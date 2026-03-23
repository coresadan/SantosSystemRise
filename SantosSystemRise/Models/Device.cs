using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SantosSystemRise.Models;

public class Device
{
    [Key] // ⭐ Ahora la MAC es el identificador único en la BD
    public string MacAddress { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /* 🔥 Propiedades de UI (No se mapean a la base de datos) */
    [NotMapped]
    public string TypeIcon { get; set; } = "bi-pc-display";

    [NotMapped]
    public bool IsSelected { get; set; } = false;
}