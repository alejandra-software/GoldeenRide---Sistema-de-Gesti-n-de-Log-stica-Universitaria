using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace GoldeenRide.Models;

[Table("vehiculos")]
public class Vehiculo : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("id_propietario")]
    public string IdPropietario { get; set; } = string.Empty;

    [Column("placa")]
    public string Placa { get; set; } = string.Empty;

    [Column("modelo")]
    public string? Modelo { get; set; }

    [Column("capacidad")]
    public int Capacidad { get; set; }

    [Column("foto_vehiculo")]
    public string? FotoVehiculo { get; set; }

    [Column("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}