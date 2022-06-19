using System.ComponentModel.DataAnnotations;

namespace WebAppProyectoDSW.Models
{
    public class Empleado
    {
        public int idEmpleado { get; set; }
        public string apeEmpleado { get; set; }
        public string nomEmpleado { get; set; }
        public DateTime fecNac { get; set; }
        [Required, StringLength(20, MinimumLength = 3)] public string correo { get; set; }
        [Required, StringLength(10, MinimumLength = 2)] public string clave { get; set; }
    }
}
