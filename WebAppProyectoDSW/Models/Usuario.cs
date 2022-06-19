using System.ComponentModel.DataAnnotations;

namespace WebAppProyectoDSW.Models
{
    public class Usuario
    {
        public int id { get; set; }
        [Required, StringLength(40, MinimumLength = 3)] public string correo { get; set; }
        [Required, MaxLength(10)] public string clave { get; set; }
    }
}
