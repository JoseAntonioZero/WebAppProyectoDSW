using System.ComponentModel.DataAnnotations;

namespace WebAppProyectoDSW.Models
{
    public class Cliente
    {
        [Display(Name = "Código Autogenerado", Order = 0)]        
        public string idCliente { get; set; }

        [Display(Name = "Nombre", Order = 1)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Ingrese el Nombre del Cliente")]
        public string nombreCliente { get; set; }

        [Display(Name = "Dirección", Order = 2)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Ingrese la Dirección del Cliente")]
        public string direccion { get; set; }

        [Display(Name = "Pais", Order = 3)]
        public int idPais { get; set; }

        [Display(Name = "Teléfono", Order = 4)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Ingrese el número de teléfono del Cliente")]
        public string telefono { get; set; }
    }
}
