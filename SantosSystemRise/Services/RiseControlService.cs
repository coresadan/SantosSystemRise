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

    // ⭐ Obtener todos los dispositivos desde SQLite
    public async Task<List<Device>> GetAllDevices()
    {
        var devices = await _context.Devices
            .AsNoTracking()
            .ToListAsync();

        // Icono por defecto (ya no hay TypeId)
        foreach (var d in devices)
            d.TypeIcon = "bi-pc-display";

        return devices;
    }

    // ⭐ Añadir un dispositivo a SQLite
    public async Task AddDevice(Device device)
    {
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
    }

    // ⭐ Actualizar un dispositivo en SQLite (versión correcta)
    public async Task UpdateDevice(Device updatedDevice)
    {
        var existing = await _context.Devices.FindAsync(updatedDevice.Id);
        if (existing == null)
            return;

        existing.Name = updatedDevice.Name;
        existing.MacAddress = updatedDevice.MacAddress;
        existing.BroadcastIP = updatedDevice.BroadcastIP;

        await _context.SaveChangesAsync();
    }

    // ⭐ Eliminar un dispositivo de SQLite
    public async Task RemoveDevice(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device != null)
        {
            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
        }
    }

    // ⭐ Wake-on-LAN
    public async Task SendMagicPacket(int deviceId)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device is null) return;

        try
        {
            string cleanMac = device.MacAddress.Replace("-", "").Replace(":", "").Replace(" ", "");
            byte[] macBytes = Convert.FromHexString(cleanMac);
            byte[] packet = new byte[102];
            Array.Fill(packet, (byte)0xff, 0, 6);
            for (int i = 0; i < 16; i++)
                Array.Copy(macBytes, 0, packet, (i + 1) * 6, 6);

            using var client = new UdpClient();
            client.EnableBroadcast = true;

            string targetBroadcast = string.IsNullOrWhiteSpace(device.BroadcastIP)
                ? "255.255.255.255"
                : device.BroadcastIP;

            var broadcastIp = IPAddress.Parse(targetBroadcast);
            await client.SendAsync(packet, packet.Length, new IPEndPoint(broadcastIp, 9));
        }
        catch
        {
            // Silencioso para mantener la UI limpia
        }
    }
}
