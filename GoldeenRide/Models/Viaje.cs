using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace GoldeenRide.Models;

[Table("viajes")]
public class Viaje : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("id_chofer")]
    public string IdChofer { get; set; } = string.Empty;

    [Column("id_vehiculo")]
    public string? IdVehiculo { get; set; }

    [Column("tipo_viaje")]
    public string TipoViaje { get; set; } = string.Empty;

    [Column("hora_salida")]
    public DateTime HoraSalida { get; set; }

    [Column("ruta_general")]
    public string RutaGeneral { get; set; } = string.Empty;

    [Column("estado")]
    public string Estado { get; set; } = string.Empty;

    [Column("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    // 👇 NUEVA COLUMNA
    [Column("dias_semana")]
    public string DiasSemana { get; set; } = string.Empty;
}