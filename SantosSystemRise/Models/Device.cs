using System.ComponentModel.DataAnnotations.Schema;

namespace SantosSystemRise.Models;

public class Device
{

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string BroadcastIP { get; set; } = string.Empty;


    /* 🔥 Estas propiedades NO se guardan en la BD */
    [NotMapped]
    public string TypeLabel { get; set; } = string.Empty;

    [NotMapped]
    public string TypeIcon { get; set; } = "bi-pc-display";

    [NotMapped]
    public bool IsSelected { get; set; } = false;
}
