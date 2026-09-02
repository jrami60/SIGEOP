using System.Text.Json.Serialization;

namespace SistemaGestionWeb.UI.Models;

public class Usuario
{
    [JsonPropertyName("id_usuario")]
    public int Id { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    // Mapea directamente el campo "nombre" de Supabase
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    public string NombreCompleto 
    {
        get => Nombre; 
        set => Nombre = value; 
    }

    [JsonPropertyName("password_hash")]
    public string Password_hash { get; set; } = string.Empty;

    [JsonPropertyName("id_rol")]
    public int RolId { get; set; }

    [JsonPropertyName("foto_url")]
    public string? FotoUrl { get; set; }
}