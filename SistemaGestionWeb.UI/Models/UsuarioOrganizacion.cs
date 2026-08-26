using System.Text.Json.Serialization;

namespace SistemaGestionWeb.UI.Models
{
    public class UsuarioOrganizacion
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("id_usuario")]
        public int IdUsuario { get; set; }

        [JsonPropertyName("organizacion_id")]
        public string OrganizacionId { get; set; } = "";

        [JsonPropertyName("rol_en_org")]
        public string RolEnOrg { get; set; } = ""; // 'admin', 'miembro', 'pendiente'
    }
}