using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SantosSystemRise.Models;

public enum DeviceStatus { Offline, Waking, Online }

public class Device
{
    [Key]
    public string MacAddress { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string IpAddress { get; set; } = string.Empty;

    /* Propiedades de UI (No se guardan en la base de datos) */
    [NotMapped]
    public DeviceStatus Status { get; set; } = DeviceStatus.Offline;

    [NotMapped]
    public string TypeIcon { get; set; } = "bi-pc-display";

    [NotMapped]
    public bool IsSelected { get; set; } = false;
}