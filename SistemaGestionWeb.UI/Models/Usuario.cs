using System.Text.Json.Serialization;

namespace SistemaGestionWeb.UI.Models;

public class Usuario
{
    [JsonPropertyName("id_usuario")]
    public int Id { get; set; } 

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("nombre")]
    public string NombreCompleto { get; set; } = string.Empty;

    [JsonPropertyName("password_hash")]
    public string Password_hash { get; set; } = string.Empty;

    [JsonPropertyName("id_rol")]
    public int RolId { get; set; }
}