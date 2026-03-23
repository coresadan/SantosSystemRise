using SantosSystemRise.Models;
using SantosSystemRise.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Sockets;

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
        // Buscamos por MacAddress porque es la Primary Key
        var existing = await _context.Devices.FindAsync(updatedDevice.MacAddress);
        if (existing != null)
        {
            existing.Name = updatedDevice.Name;
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
            var broadcastIp = IPAddress.Broadcast; // 255.255.255.255

            await client.SendAsync(packet, packet.Length, new IPEndPoint(broadcastIp, 9));
        }
        catch (Exception)
        {
            // Fallo silencioso para no interrumpir la UI
        }
    }
}