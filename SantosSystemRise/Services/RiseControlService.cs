using SantosSystemRise.Models;
using SantosSystemRise.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation; // ⭐ Añadido para el Ping

namespace SantosSystemRise.Services;

public class RiseControlService
{
    private readonly SystemRiseContext _context;

    public RiseControlService(SystemRiseContext context)
    {
        _context = context;
    }

    public async Task<List<Device>> GetAllDevices()
    {
        var devices = await _context.Devices.AsNoTracking().ToListAsync();
        foreach (var d in devices) d.TypeIcon = "bi-pc-display";
        return devices;
    }

    public async Task AddDevice(Device device)
    {
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDevice(Device updatedDevice)
    {
        var existing = await _context.Devices.FindAsync(updatedDevice.MacAddress);
        if (existing != null)
        {
            existing.Name = updatedDevice.Name;
            existing.IpAddress = updatedDevice.IpAddress;

            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveDevice(string mac)
    {
        var device = await _context.Devices.FindAsync(mac);
        if (device != null)
        {
            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
        }
    }

    // ⭐ NUEVO MÉTODO: Solo para consultar si responde en la red
    public async Task<bool> IsDeviceOnline(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) return false;
        try
        {
            using var ping = new Ping();
            // 800ms de tiempo de espera es ideal para redes de empresa
            var reply = await ping.SendPingAsync(ipAddress, 800);
            return reply.Status == IPStatus.Success;
        }
        catch { return false; }
    }

    public async Task SendMagicPacket(string mac)
    {
        if (string.IsNullOrWhiteSpace(mac)) return;
        try
        {
            // Limpieza más robusta para evitar fallos por caracteres raros
            string cleanMac = new string(mac.Where(char.IsLetterOrDigit).ToArray());
            if (cleanMac.Length != 12) return;

            byte[] macBytes = Convert.FromHexString(cleanMac);
            byte[] packet = new byte[102];
            Array.Fill(packet, (byte)0xff, 0, 6);
            for (int i = 0; i < 16; i++)
                Array.Copy(macBytes, 0, packet, (i + 1) * 6, 6);

            using var client = new UdpClient();
            client.EnableBroadcast = true;
            await client.SendAsync(packet, packet.Length, new IPEndPoint(IPAddress.Broadcast, 9));
        }
        catch { /* Silencio */ }
    }
}