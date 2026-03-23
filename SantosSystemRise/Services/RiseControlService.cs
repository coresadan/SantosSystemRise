using SantosSystemRise.Models;
using SantosSystemRise.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation; // 👈 NUEVO: Para usar la clase Ping

namespace SantosSystemRise.Services;

public class RiseControlService
{
    private readonly SystemRiseContext _context;

    public RiseControlService(SystemRiseContext context)
    {
        _context = context;
    }

    // --- MÉTODOS DE BASE DE DATOS ---

    public async Task<List<Device>> GetAllDevices()
    {
        var devices = await _context.Devices.AsNoTracking().ToListAsync();
        // Mantenemos tu lógica de iconos por defecto
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
            existing.IpAddress = updatedDevice.IpAddress; // 👈 NUEVO: Actualizamos también la IP
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

    // --- ⭐ NUEVO: DETECCIÓN REAL POR PING (IP) ---
    // Este método es el que usará la IP de Herrajes para saber si está ON/OFF
    public async Task<bool> IsDeviceOnline(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) return false;

        try
        {
            using var ping = new Ping();
            // Enviamos un paquete y esperamos máximo 500ms
            // Es mucho más fiable que el comando ARP
            var reply = await ping.SendPingAsync(ipAddress, 500);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    // --- LÓGICA DE ENCENDIDO (MAGIC PACKET) ---
    public async Task SendMagicPacket(string mac)
    {
        if (string.IsNullOrWhiteSpace(mac)) return;

        try
        {
            string cleanMac = mac.Replace("-", "").Replace(":", "").Replace(" ", "");
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
        catch { /* Silencioso */ }
    }
}