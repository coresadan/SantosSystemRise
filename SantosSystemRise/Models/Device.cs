using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SantosSystemRise.Models;

// Añadimos este pequeño "diccionario" de estados fuera de la clase
public enum DeviceStatus { Offline, Online, Waking }

public class Device
{
    [Key]
    public string MacAddress { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;

    /* 🔥 Propiedades de UI (No se mapean a la base de datos) */
    [NotMapped]
    public string TypeIcon { get; set; } = "bi-pc-display";

    [NotMapped]
    public bool IsSelected { get; set; } = false;

    [NotMapped]
    public DeviceStatus Status { get; set; } = DeviceStatus.Offline;
}