using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
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

        string verifica(string correo, string clave)
        {
            string sw = "";
            using (SqlConnection cn = new conexion().getcn)
            {
                cn.Open();

                //ejecutar el procedure usp_verifica_acceso, pasando sus parametros de entrada y salida
                SqlCommand cmd = new SqlCommand("usp_verifica_acceso", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@clave", clave);
                cmd.Parameters.Add("@sw", SqlDbType.VarChar, 1).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@fullname", SqlDbType.VarChar, 150).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();

                //al ejecutar los parametros de salida seran almacenados
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
            return View(await Task.Run(() => new Usuario()));
        }

        [HttpPost]public async Task<IActionResult> Login(Usuario reg)
        {
            //Valido si los datos en el formulario son correctos
            if (!ModelState.IsValid) return View(await Task.Run(() => reg));

            //Si se ingresaron datos correctos
            string sw = verifica(reg.correo, reg.clave);
            if (sw == "0") // -> 0 = no existe empleado con esos datos
            {
                ModelState.AddModelError("", HttpContext.Session.GetString(sesion));
                return View(await Task.Run(() => reg));
            }
            else // -> 1 = Datos correctos
            {
                return RedirectToAction("MenuPrincipal");
            }
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

            ViewBag.usuario = HttpContext.Session.GetString(sesion);
            return View(listadoProducto());
        }
        /* ---------------------------  JOSÉ  ---------------------------*/
        //REALIZAR PEDIDO (Al agregar pedido se actualiza el stock de productos)

        /*
         * dsfsdfsdfsd
         * 
         * 
         * 
         * 
         * sdfdsafsdfdsf
         */

        //REPORTE DE PEDIDOS

        /*prueba jose2
         * 
         * sdafsdfsdfsdfsddsf
         * 
         * 
         * 
         * 
         * dsfsaddsfsdf
         * 
         */



        /* ---------------------------  ALAIN  ---------------------------*/
        //MANTENIMIENTO DE CLIENTES (formulario y listado)


        /*alakings
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         */




        //REPORTE DE CLIENTES (listado por consulta)



        /* ---------------------------  HILLARY  ---------------------------*/
        //MANTENIMIENTO DE PRODUCTOS (formulario y listado)



        //REPORTE DE PRODUCTOS (listado por consulta)



        /* ---------------------------  JESÚS  ---------------------------*/
        //MANTENIMIENTO DE PROVEEDORES (formulario y listado)
        //Jechu
       IEnumerable<Pais> paises()
        {
            List<Pais> temporal = new List<Pais>();
            using (SqlConnection cn = new conexion().getcn)
            {
                SqlCommand cmd = new SqlCommand("exec usp_paises",cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    temporal.Add(new Pais()
                    {
                        idPais = dr.GetInt32(0),
                        nombrePais = dr.GetString(1)
                        
                    });
                }
            }
            return temporal;
        }

        //
        //




        //REPORTE DE PROVEEDORES (listado por consulta)



        /* ---------------------------  LADY  ---------------------------*/
        //MANTENIMIENTO DE EMPLEADOS (formulario y listado)

        //A



        //B



        //C

        //REPORTE DE EMPLEADOS



        
    }
}
