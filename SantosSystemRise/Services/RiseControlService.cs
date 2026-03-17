using SantosSystemRise.Models;
using System.Net;
using System.Net.Sockets;

namespace SantosSystemRise.Services;

public class RiseControlService
{
    private readonly List<Device> _devices = [];
    private readonly List<DeviceType> _types = [];

    public RiseControlService()
    {
        _types.AddRange([
            new DeviceType { Id = 1, Label = "Estación de Control", Icon = "bi-display" },
            new DeviceType { Id = 2, Label = "Servidor Local", Icon = "bi-pc-display-horizontal" },
            new DeviceType { Id = 3, Label = "Maquinaria CNC", Icon = "bi-gear-fill" }
        ]);

        _devices.Add(new Device
        {
            Id = 1,
            Name = "ASRock Z77 Pro4 - Admin",
            Hostname = "ASROCK-ADMIN",
            MacAddress = "BC-5F-F4-90-9E-53",
            BroadcastIP = "172.16.255.255",
            TypeId = 1
        });
    }

    public IEnumerable<Device> GetAllDevices()
    {
        return from d in _devices
               join t in _types on d.TypeId equals t.Id
               select new Device
               {
                   Id = d.Id,
                   Name = d.Name,
                   Hostname = d.Hostname,
                   MacAddress = d.MacAddress,
                   BroadcastIP = d.BroadcastIP,
                   TypeId = d.TypeId,
                   TypeIcon = t.Icon
                   // Eliminamos TypeLabel e IP para limpiar la UI
               };
    }

    public void AddDevice(Device device)
    {
        device.Id = _devices.Count != 0 ? _devices.Max(x => x.Id) + 1 : 1;
        _devices.Add(device);
    }

    public void UpdateDevice(Device updatedDevice)
    {
        var existing = _devices.FirstOrDefault(d => d.Id == updatedDevice.Id);
        if (existing != null)
        {
            existing.Name = updatedDevice.Name;
            existing.MacAddress = updatedDevice.MacAddress;
            existing.BroadcastIP = updatedDevice.BroadcastIP;
            existing.TypeId = updatedDevice.TypeId;
            existing.Hostname = updatedDevice.Hostname;
        }
    }

    public void RemoveDevice(int id) => _devices.RemoveAll(x => x.Id == id);

    public async Task SendMagicPacket(int deviceId)
    {
        var device = _devices.FirstOrDefault(d => d.Id == deviceId);
        if (device is null) return;

        try
        {
            string cleanMac = device.MacAddress.Replace("-", "").Replace(":", "").Replace(" ", "");
            byte[] macBytes = Convert.FromHexString(cleanMac);
            byte[] packet = new byte[102];
            Array.Fill(packet, (byte)0xff, 0, 6);
            for (int i = 0; i < 16; i++) Array.Copy(macBytes, 0, packet, (i + 1) * 6, 6);

            using var client = new UdpClient();
            client.EnableBroadcast = true;
            string targetBroadcast = string.IsNullOrWhiteSpace(device.BroadcastIP) ? "255.255.255.255" : device.BroadcastIP;
            var broadcastIp = IPAddress.Parse(targetBroadcast);
            await client.SendAsync(packet, packet.Length, new IPEndPoint(broadcastIp, 9));
        }
        catch (Exception) { /* Silencioso para mantener la UI limpia */ }
    }
}