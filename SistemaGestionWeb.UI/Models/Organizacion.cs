namespace SistemaGestionWeb.UI.Models
{
    public class Organizacion
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Rol { get; set; } = "Administrador";

        public bool PermitirCrearTareas { get; set; } = true;
        public bool PermitirEditarEliminar { get; set; } = true;
    }
    public class OrgConMiembrosDto
    {
        public string IdOrganizacion { get; set; } = string.Empty;
        public string NombreOrganizacion { get; set; } = string.Empty;
        public string RolUsuarioActual { get; set; } = string.Empty;
        public List<MiembroModel> Miembros { get; set; } = new();
    }
}