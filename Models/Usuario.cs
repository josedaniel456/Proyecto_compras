using System.ComponentModel.DataAnnotations;

namespace Proyecto_compras.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required, EmailAddress]
        public string Correo { get; set; }

        [Required, DataType(DataType.Password)]
        public string Contraseña { get; set; }

        public bool Estado { get; set; } = true;

        public string Rol { get; set; } = "usuario";
    }
}
