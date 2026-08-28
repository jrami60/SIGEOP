namespace SistemaGestionWeb.UI.Models;

public class MiembroModel
{
    [System.Text.Json.Serialization.JsonPropertyName("usuarios")]
    public UsuarioData? Usuario { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("id_usuario")]
    public int IdUsuario { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("id_relacion")]
    public int IdRelacion { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("rol_en_org")]
    public string Rol { get; set; } = string.Empty;
    public string Nombre => Usuario?.Nombre ?? "Sin nombre";
    public string Estado { get; set; } = "Activo";

    public string NombreUsuario => Nombre;
    public string RolEnOrg => Rol;
}

public class UsuarioData
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public int Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    public string? foto_url { get; set; }
}