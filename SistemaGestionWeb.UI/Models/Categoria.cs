namespace SistemaGestionWeb.UI.Models;

using System.Text.Json.Serialization;

public class Categoria
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("color_hex")]
    public string? ColorHex { get; set; }

    [JsonPropertyName("creado_en")]
    public DateTime? CreadoEn { get; set; }

    public string OrganizacionId { get; set; } = string.Empty;
}