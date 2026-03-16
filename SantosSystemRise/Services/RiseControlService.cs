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

        // Dispositivo de prueba con la MAC de tu ASRock Z77 Pro4
        _devices.Add(new Device
        {
            Id = 1,
            Name = "ASRock Z77 Pro4 - Admin",
            Hostname = "ASROCK-ADMIN",
            MacAddress = "BC-5F-F4-90-9E-53",
            BroadcastIP = "172.16.255.255", // Ahora es dinámico por dispositivo
            TypeId = 1
        });
    }

    public IEnumerable<dynamic> GetAllDevices()
    {
        // Mantenemos tu sistema LINQ con join, que es muy limpio
        return from d in _devices
               join t in _types on d.TypeId equals t.Id
               select new
               {
                   d.Id,
                   d.Name,
                   d.Hostname,
                   d.MacAddress,
                   d.IP,
                   d.IsActive,
                   d.BroadcastIP, // Incluimos esto para la UI si fuera necesario
                   TypeLabel = t.Label,
                   TypeIcon = t.Icon
               };
    }

    public async Task SendMagicPacket(int deviceId)
    {
        var device = _devices.FirstOrDefault(d => d.Id == deviceId);
        if (device is null) return;

        try
        {
            // Limpiamos la MAC para aceptar formatos con guiones, puntos o espacios
            string cleanMac = device.MacAddress.Replace("-", "").Replace(":", "").Replace(" ", "");
            byte[] macBytes = Convert.FromHexString(cleanMac);

            byte[] packet = new byte[102];
            Array.Fill(packet, (byte)0xff, 0, 6);
            for (int i = 0; i < 16; i++) Array.Copy(macBytes, 0, packet, (i + 1) * 6, 6);

            using var client = new UdpClient();
            client.EnableBroadcast = true;

            // MEJORA: Usamos la IP de Broadcast guardada en el dispositivo. 
            // Si no tiene una, usamos la genérica por defecto.
            string targetBroadcast = string.IsNullOrWhiteSpace(device.BroadcastIP)
                                     ? "255.255.255.255"
                                     : device.BroadcastIP;

            var broadcastIp = IPAddress.Parse(targetBroadcast);

            // Enviamos el pulso
            await client.SendAsync(packet, packet.Length, new IPEndPoint(broadcastIp, 9));

            // Actualizamos estado visual de envío
            device.IP = "Pulso enviado...";

            // Verificación asíncrona tras el arranque (esperamos 20 seg por ser Windows 7)
            _ = Task.Run(async () => {
                await Task.Delay(20000);
                await ResolveIpAsync(device);
            });
        }
        catch (Exception ex)
        {
            device.IP = "Error: " + ex.Message;
        }
    }

    public async Task ResolveIpAsync(Device device)
    {
        if (string.IsNullOrWhiteSpace(device.Hostname)) return;
        try
        {
            var host = await Dns.GetHostEntryAsync(device.Hostname);
            var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            if (ip is not null)
            {
                device.IP = ip.ToString();
                device.IsActive = true;
            }
        }
        catch
        {
            device.IP = "Sin respuesta (WOL enviado)";
            device.IsActive = false;
        }
    }

    public void AddDevice(Device device)
    {
        device.Id = _devices.Count != 0 ? _devices.Max(x => x.Id) + 1 : 1;
        _devices.Add(device);
    }

    public void RemoveDevice(int id) => _devices.RemoveAll(x => x.Id == id);
}