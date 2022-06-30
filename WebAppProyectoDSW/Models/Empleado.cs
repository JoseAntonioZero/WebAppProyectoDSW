using System.ComponentModel.DataAnnotations;

namespace WebAppProyectoDSW.Models
{
    public class Empleado
    {
        public int idEmpleado { get; set; }
        public string apeEmpleado { get; set; }
        public string nomEmpleado { get; set; }
        public DateTime fecNac { get; set; }
        public DateTime fecCon { get; set; }
        [Required, StringLength(40, MinimumLength = 3)] public string correo { get; set; }
        [Required, MaxLength(10)] public string clave { get; set; }
    }
}
