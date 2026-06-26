using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace GoldeenRide.Models;

[Table("reservas")]
public class Reserva : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("id_viaje")]
    public string IdViaje { get; set; } = string.Empty;

    [Column("id_pasajero")]
    public string IdPasajero { get; set; } = string.Empty;

    [Column("punto_recogida_texto")]
    public string PuntoRecogidaTexto { get; set; } = string.Empty;

    [Column("latitud")]
    public double? Latitud { get; set; }

    [Column("longitud")]
    public double? Longitud { get; set; }

    [Column("hora_estimada_recogida")]
    public DateTime? HoraEstimadaRecogida { get; set; }

    [Column("estado")]
    public string Estado { get; set; } = string.Empty;

    [Column("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}