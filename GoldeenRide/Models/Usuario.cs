using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace GoldeenRide.Models;

[Table("usuarios")]
public class Usuario : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("rol")]
    public string Rol { get; set; } = string.Empty;

    [Column("foto_perfil")]
    public string? FotoPerfil { get; set; }

    [Column("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    [Column("id_jefe")]
    public string? IdJefe { get; set; }
}