namespace SantosSystemRise.Models;

public class DeviceType
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty; // Ej: "Maquinaria CNC"
    public string Icon { get; set; } = string.Empty;  // Ej: "bi-gear-fill"
}