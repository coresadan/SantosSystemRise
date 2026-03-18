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

        // Icono por defecto
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

    // ⭐ Actualizar un dispositivo en SQLite
    public async Task UpdateDevice(Device updatedDevice)
    {
        var existing = await _context.Devices.FindAsync(updatedDevice.Id);
        if (existing == null)
            return;

        existing.Name = updatedDevice.Name;
        existing.MacAddress = updatedDevice.MacAddress;
        // BroadcastIP eliminado de la actualización

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

    // ⭐ Wake-on-LAN (Versión Adaptativa)
    public async Task SendMagicPacket(int deviceId)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device is null) return;

        try
        {
            // Limpieza de la dirección MAC
            string cleanMac = device.MacAddress.Replace("-", "").Replace(":", "").Replace(" ", "");

            // Validación básica de longitud MAC
            if (cleanMac.Length != 12) return;

            byte[] macBytes = Convert.FromHexString(cleanMac);
            byte[] packet = new byte[102];

            // Cabecera: 6 bytes de 0xFF
            Array.Fill(packet, (byte)0xff, 0, 6);

            // Cuerpo: Repetir la MAC 16 veces
            for (int i = 0; i < 16; i++)
                Array.Copy(macBytes, 0, packet, (i + 1) * 6, 6);

            using var client = new UdpClient();
            client.EnableBroadcast = true;

            var broadcastIp = IPAddress.Broadcast; // 255.255.255.255

            await client.SendAsync(packet, packet.Length, new IPEndPoint(broadcastIp, 9));
        }
        catch
        {
            // Silencioso para mantener la UI limpia
        }
    }
}