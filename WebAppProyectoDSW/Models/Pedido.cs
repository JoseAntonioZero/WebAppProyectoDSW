namespace WebAppProyectoDSW.Models
{
    public class Pedido
    {
        public string idPedido { get; set; }
        public string idCliente { get; set; }
        public int idEmpleado { get; set; }
        public int idProducto { get; set; }
        public DateTime fechaPedido { get; set; }
        public decimal precioUnidad { get; set; }
        public int cantidad { get; set; }
        public decimal monto { get; set; }
    }
}
