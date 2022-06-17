using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Session;
using Newtonsoft.Json;
using WebAppProyectoDSW.Models;
using WebAppProyectoDSW.Util;

namespace WebAppProyectoDSW.Controllers
{
    public class MarketecController : Controller
    {
        //cadena = Carpeta Util > Conexion


        /*
         public IEnumerable<Producto> listado()
        {
            List<Producto> auxiliar = new List<Producto>();
            using (SqlConnection cn = new conexionDAO().getcn)
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("exec usp_productos_inventario", cn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    auxiliar.Add(new Producto()
                    {
                        idproducto = dr.GetInt32(0),
                        descripcion = dr.GetString(1),
                        categoria = dr.GetString(2),
                        precio = dr.GetDecimal(3),
                        unidades = dr.GetInt16(4) //smallint
                    });
                }
            }
            return auxiliar;
        }
         */
        public IActionResult Index()
        {
            return View();
        }
    }
}
