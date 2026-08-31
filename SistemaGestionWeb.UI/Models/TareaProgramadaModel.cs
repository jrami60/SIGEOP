using System.Text.Json.Serialization;

namespace SistemaGestionWeb.UI.Models;

public class TareaProgramadaModel
{
    [JsonPropertyName("id_registro")]
    public int Id { get; set; }

    [JsonPropertyName("organizacion_id")]
    public string OrganizacionId { get; set; } = string.Empty;

    [JsonPropertyName("id_usuario")]
    public int? IdUsuario { get; set; }

    [JsonPropertyName("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [JsonPropertyName("prioridad")]
    public string Prioridad { get; set; } = "Media";

    [JsonPropertyName("tipo_tarea")]
    public string TipoTarea { get; set; } = "programada";

    [JsonPropertyName("fecha_programada")]
    public DateTime? FechaProgramada { get; set; } = DateTime.Now.AddDays(1);
}