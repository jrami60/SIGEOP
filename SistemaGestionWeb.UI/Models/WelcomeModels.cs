using System.ComponentModel.DataAnnotations;

namespace SistemaGestionWeb.UI.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = "";
    }

    public class RegistroModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string NombreCompleto { get; set; } = "";

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "Debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirma tu contraseña.")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } = "";
    }

    public class UsuarioRegistroDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "Debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = "";
    }
}