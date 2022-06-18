﻿namespace WebAppProyectoDSW.Models
{
    public class Carrito
    {
        public int idProducto { get; set; }
        public string nombreProducto { get; set; }
        public int idProveedor { get; set; }
        public int idCategoria { get; set; }
        public decimal precioUnidad { get; set; }
        public int stock { get; set; }
        public decimal monto { get {return precioUnidad * stock; } }
    }
}
