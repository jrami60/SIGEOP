namespace SistemaGestionWeb.UI.Models;

using System.Text.Json.Serialization;

public class Registro
{
    [JsonPropertyName("id_registro")]
    public int Id { get; set; }

    [JsonPropertyName("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [JsonPropertyName("id_categoria")]
    public int CategoriaId { get; set; }

    [JsonPropertyName("id_estado")]
    public int EstadoId { get; set; }

    [JsonPropertyName("organizacion_id")]
    public string OrganizacionId { get; set; } = string.Empty;

    public string? Observaciones { get; set; }
    public string? FotoEvidencia { get; set; }

    [JsonPropertyName("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [JsonPropertyName("foto_url")]
    public string? FotoUrl { get; set; }

    [JsonPropertyName("id_usuario")]
    public int? UsuarioId { get; set; }
}



