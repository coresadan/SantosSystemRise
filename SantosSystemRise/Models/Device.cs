namespace SantosSystemRise.Models;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;

    public string BroadcastIP { get; set; } = "255.255.255.255";

    public string IP { get; set; } = "Buscando...";
    public int TypeId { get; set; } = 1;
    public bool IsActive { get; set; }
}