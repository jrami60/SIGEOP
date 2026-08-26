namespace SistemaGestionWeb.UI.Models;

public class Comentario
{
    public int Id { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    
    public int RegistroId { get; set; }
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}