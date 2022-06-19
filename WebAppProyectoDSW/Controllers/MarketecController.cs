using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using WebAppProyectoDSW.Models;
using WebAppProyectoDSW.Util;

namespace WebAppProyectoDSW.Controllers
{
    public class MarketecController : Controller
    {
        string sesion = ""; 

        string verificar(string correo, string clave)
        {
            string sw = "";
            using (SqlConnection cn = new conexion().getcn)
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("usp_verifica_acceso", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@clave", clave);
                cmd.Parameters.Add("@sw", SqlDbType.VarChar, 1).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@fullname", SqlDbType.VarChar, 150).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();

                sw = cmd.Parameters["@sw"].Value.ToString();
                HttpContext.Session.SetString(sesion, cmd.Parameters["@fullname"].Value.ToString());
            }
            return sw;
        }

        public async Task<IActionResult> Login()
        {
            //inicializar el Session 
            HttpContext.Session.SetString(sesion, "");

            //envio un nuevo usuario
            return View(await Task.Run(() => new Empleado()));
        }

        // Listado de productos PARA EL MENÚ
        public IEnumerable<Producto> listadoProducto()
        {
            List<Producto> temporal = new List<Producto>();
            using (SqlConnection cn = new conexion().getcn)
            {
                SqlCommand cmd = new SqlCommand("exec usp_listar_productos", cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    temporal.Add(new Producto()
                    {
                        idProducto = dr.GetInt32(0),
                        nombreProducto = dr.GetString(1),
                        idProveedor = dr.GetInt32(2),
                        idCategoria = dr.GetInt32(3),
                        precioUnidad = dr.GetDecimal(4),
                        stock = dr.GetInt16(5)
                    });
                }
            }
            return temporal;
        }

        Producto Buscar(int codigo = 0)
        {
            return listadoProducto().FirstOrDefault(c => c.idProducto == codigo);
        }

        public IActionResult MenuPrincipal()
        {
            /*
            //evaluo, si no existe Session carrito, definirlo como una lista de Pedido vacio
            if (HttpContext.Session.GetString("carrito") == null)
            {
                HttpContext.Session.SetString("carrito",
                    JsonConvert.SerializeObject(new List<Carrito>()));
            }
            */
            //envio la lista de productos

            //ViewBag.empleado = HttpContext.Session.GetString(logSession);
            return View(listadoProducto());
        }


    }
}
