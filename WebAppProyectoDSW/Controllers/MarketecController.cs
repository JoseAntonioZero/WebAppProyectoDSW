using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using System.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using WebAppProyectoDSW.Models;
using WebAppProyectoDSW.Util;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebAppProyectoDSW.Controllers
{
    public class MarketecController : Controller
    {


        string sesion = "";
        string sesioncod = "";

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
                cmd.Parameters.Add("@codigo", SqlDbType.VarChar, 1).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();

                //al ejecutar los parametros de salida seran almacenados
                sw = cmd.Parameters["@sw"].Value.ToString();
                HttpContext.Session.SetString(sesion, cmd.Parameters["@fullname"].Value.ToString());
                //HttpContext.Session.SetString(sesioncod, cmd.Parameters["@codigo"].Value.ToString());
            }
            return sw;
        }

        public async Task<IActionResult> Login()
        {
            //inicializar el Session 
            HttpContext.Session.SetString(sesion, "");
            //HttpContext.Session.SetString(sesioncod, "");
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
            
            //evaluo, si no existe Session carrito, definirlo como una lista de Pedido vacio
            if (HttpContext.Session.GetString("carrito") == null)
            {
                HttpContext.Session.SetString("carrito",
                    JsonConvert.SerializeObject(new List<Carrito>()));
            }
            
            //envio la lista de productos

            ViewBag.usuario = HttpContext.Session.GetString(sesion);
            return View(listadoProducto());
        }
        /* ---------------------------  JOSÉ  ---------------------------*/
        //REALIZAR PEDIDO (Al agregar pedido se actualiza el stock de productos)

        public IActionResult Seleccionar(int id = 0)
        {
            //buscar el producto por idproducto
            Producto reg = Buscar(id);
            if (reg == null)
            {
                return RedirectToAction("MenuPrincipal");
            }
            else
            {
                return View(reg);
            }
        }

        [HttpPost]public IActionResult Seleccionar(int codigo, int cantidad)
        {
            //buscar el producto por su idproducto
            Producto reg = Buscar(codigo);
            //en un Registro almaceno los datos
            Carrito item = new Carrito() { 
                idProducto=reg.idProducto,
                nombreProducto=reg.nombreProducto,
                idProveedor=reg.idProveedor,
                idCategoria=reg.idCategoria,
                precioUnidad=reg.precioUnidad,
                stock=cantidad,
            };

            //para almacenarlo en Session "carrito" debo deserializar
            List<Carrito> auxiliar = JsonConvert.DeserializeObject<List<Carrito>>(
                HttpContext.Session.GetString("carrito"));

            auxiliar.Add(item);

            //volver a serializar auxiliar almacenando en el Session
            HttpContext.Session.SetString("carrito", JsonConvert.SerializeObject(auxiliar));

            ViewBag.mensaje = "Producto Agregado";
            return View(reg); //envias a reg (su clase es de tipo Producto
        }

        public IActionResult Carrito()
        {
            //deserializar el Sesion "carrito"
            List<Carrito> auxiliar = JsonConvert.DeserializeObject<List<Carrito>>(
                HttpContext.Session.GetString("carrito"));

            //si auxiliar esta vacio, regresar al Portal
            if (auxiliar.Count == 0)
                return RedirectToAction("MenuPrincipal");
            else
                return View(auxiliar);
        }

        public IActionResult Delete(int id)
        {
            //1.deserializar el Sesion Canasta
            List<Carrito> auxiliar = JsonConvert.DeserializeObject<List<Carrito>>(
              HttpContext.Session.GetString("carrito"));

            //2.buscar el registro por su codigo
            Carrito reg = auxiliar.FirstOrDefault(x => x.idProducto == id);
            //3.eliminar
            auxiliar.Remove(reg);
            //4.volver a serializar
            HttpContext.Session.SetString("carrito", JsonConvert.SerializeObject(auxiliar));
            //ir a la vista Canasta
            return RedirectToAction("carrito");
        }

        public IActionResult Registro()
        {
            //deserializar el Sesion Canasta
            List<Carrito> auxiliar = JsonConvert.DeserializeObject<List<Carrito>>(
              HttpContext.Session.GetString("carrito"));
            //envio los registros del Session canasta

            ViewBag.clientes = new SelectList(ListadoClientes(), "idCliente", "nombreCliente");
            ViewBag.empleados = new SelectList(ListadoEmpleados(), "idEmpleado", "nomEmpleado");
            return View(auxiliar);
        }

        [HttpPost]
        public IActionResult Registro(string idcliente, string idempleado)
        {
            string mensaje = "";
            using (SqlConnection cn = new conexion().getcn)
            {
                //lista de Session
                List<Carrito> auxiliar = JsonConvert.DeserializeObject<List<Carrito>>(
                  HttpContext.Session.GetString("carrito"));
                //abrir la conexion para definir la transaccion
                cn.Open();
                SqlTransaction tr = cn.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                    //ejecutando el procedure de tb_pedidos
                    SqlCommand cmd = new SqlCommand("usp_agregar_pedidos", cn, tr);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@idpedido", SqlDbType.VarChar, 5).Direction = ParameterDirection.Output;
                    cmd.Parameters.AddWithValue("@idCliente", idcliente);
                    cmd.Parameters.AddWithValue("@idEmpleado", idempleado);
                    cmd.Parameters.AddWithValue("@monto", auxiliar.Sum(x => x.monto));
                    cmd.ExecuteNonQuery();

                    //despues de ejecutar recupero el valor de @npedido output
                    string idpedido = cmd.Parameters["@idpedido"].Value.ToString();

                    //almacenando el detalle, leer cada registro del auxiliar y ejecuto el procedure
                    auxiliar.ForEach(reg =>
                    {
                        cmd = new SqlCommand("exec usp_agregar_detallepedido @idpedido, @idProducto, @precio,@cantidad", cn, tr);
                        cmd.Parameters.AddWithValue("@idpedido", idpedido);
                        cmd.Parameters.AddWithValue("@idproducto", reg.idProducto);
                        cmd.Parameters.AddWithValue("@precio", reg.precioUnidad);
                        cmd.Parameters.AddWithValue("@cantidad", reg.stock);
                        cmd.ExecuteNonQuery();
                    });

                    //actualizar el stock
                    auxiliar.ForEach(reg =>
                    {
                        cmd = new SqlCommand("exec usp_actualizar_stock @idproducto, @cantidad", cn, tr);
                        cmd.Parameters.AddWithValue("@idproducto", reg.idProducto);
                        cmd.Parameters.AddWithValue("@cantidad", (Int16)reg.stock); //lo envio como smallint
                        cmd.ExecuteNonQuery();
                    });

                    //si todo esta OK
                    tr.Commit();
                    mensaje = $"Se ha registrado el pedido {idpedido}";
                }

                catch (Exception ex)
                {
                    mensaje = ex.Message;
                    tr.Rollback(); //en caso de error deshacer las operaciones
                }
                finally { cn.Close(); }
            }

            //Vacía carrito
            HttpContext.Session.SetString("carrito", JsonConvert.SerializeObject(new List<Carrito>()));
            //al finalizar redirecciona hacia ventanas con el mensaje
            return RedirectToAction("Confirmacion", new { msg = mensaje });
            
        }

        public IActionResult Confirmacion(string msg)
        {
            ViewBag.msg = msg;
            return View();
        }

        /*
         * dsfsdfsdfsd
         * 
         * 
         * 
         * 
         * sdfdsafsdfdsf
         */

        //REPORTE DE PEDIDOS

        IEnumerable<Pedido> listarPedidosXProducto(int id)
        {

            List<Pedido> listPedido = new List<Pedido>();
            using (SqlConnection cn = new conexion().getcn)
            {

                SqlCommand cmd = new SqlCommand("exec usp_listar_pedidosXproducto @id", cn);
                cmd.Parameters.AddWithValue("@id", id);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    listPedido.Add(new Pedido()
                    {
                        idPedido = dr.GetString(0),
                        idCliente = dr.GetString(1),
                        idEmpleado = dr.GetInt32(2),
                        fechaPedido = dr.GetDateTime(3),
                        monto = dr.GetDecimal(4),
                        nomProducto = dr.GetString(5),
                        cantidad = dr.GetInt32(6)
                    });
                }
                dr.Close(); cn.Close();
            }
            return listPedido;
        }

        public async Task<IActionResult> reportePedidosXProductos(int id = 0)
        {   
            IEnumerable<Pedido> reporte = listarPedidosXProducto(id);
            ViewBag.productos = new SelectList(listadoProducto(), "idProducto", "nombreProducto");

            return View(await Task.Run(() => reporte));
        }



        /* ---------------------------  ALAIN  -----------------------------------------------------------*/
        //MANTENIMIENTO DE CLIENTES (formulario y listado)

        public IEnumerable<Cliente> ListadoClientes()
        {
            List<Cliente> listaCli = new List<Cliente>();
            using (SqlConnection cn = new conexion().getcn)
            {
                SqlCommand cmd = new SqlCommand("exec usp_listar_clientes", cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    listaCli.Add(new Cliente()
                    {
                        idCliente = dr.GetString(0),
                        nombreCliente = dr.GetString(1),
                        direccion = dr.GetString(2),
                        idPais = dr.GetInt32(3),
                        telefono = dr.GetString(4)                        
                    });
                }
            }
            return listaCli;
        }

        //CONSULTAR CLIENTES
        public async Task<IActionResult> ListarClientes()
        {
            IEnumerable<Cliente> lstCli = ListadoClientes();

            return View(await Task.Run(() => lstCli));
        }

        //BUSCAR CLIENTE
        Cliente BuscarCliente(String id)
        {
            //String codigo = id.ToString();

            if (string.IsNullOrEmpty(id))
                return new Cliente();
            else
                return ListadoClientes().FirstOrDefault(c => c.idCliente == id);

        }

        //Listar Paises Clientes
        IEnumerable<Pais> ListarPaises()
        {
            List<Pais> listPaises = new List<Pais>();
            using (SqlConnection cn = new conexion().getcn)
            {
                SqlCommand cmd = new SqlCommand("exec usp_listar_paises", cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    listPaises.Add(new Pais()
                    {
                        idPais = dr.GetInt32(0),
                        nombrePais = dr.GetString(1)

                    });
                }
            }
            return listPaises;
        }

        //REGISTRAR CLIENTE
        public async Task<IActionResult> RegistrarCliente()
        {            
            ViewBag.paises = new SelectList(ListarPaises(), "idPais", "nombrePais");
            return View(new Cliente());
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarCliente(Cliente cli)
        {
            
            string mensaje = "";
                using (SqlConnection cn = new conexion().getcn)
                {
                    cn.Open();
                    SqlTransaction tst = cn.BeginTransaction(IsolationLevel.Serializable);
                    try
                    {

                        SqlCommand cmd = new SqlCommand("usp_agregar_cliente", cn, tst);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id", SqlDbType.VarChar, 5).Direction = ParameterDirection.Output;
                        cmd.Parameters.AddWithValue("@nombre", cli.nombreCliente);
                        cmd.Parameters.AddWithValue("@direccion", cli.direccion);
                        cmd.Parameters.AddWithValue("@pais", cli.idPais);
                        cmd.Parameters.AddWithValue("@telefono", cli.telefono);
                        cmd.ExecuteNonQuery();

                        string ncliente = cmd.Parameters["@id"].Value.ToString();

                        tst.Commit();
                        mensaje = $"Cliente: '{cli.nombreCliente}' se registró como {ncliente} ";                        

                }
                    catch (Exception ex)
                    {
                        mensaje = ex.Message;
                        tst.Rollback();
                    }
                    finally { cn.Close(); }

                }

                ViewBag.mensaje = mensaje;

            return View(await Task.Run(() => cli));
        }

        public async Task<IActionResult> ActualizarCliente(String id)
        {
            Cliente cli = BuscarCliente(id);

            if (cli == null)
            {                
                return RedirectToAction("ListarClientes");
            }
            else
            {
                ViewBag.paises = new SelectList(ListarPaises(), "idPais", "nombrePais");
                return View(cli);
            }
            return View(await Task.Run(() => cli));

        }

        [HttpPost]
        public async Task<IActionResult> ActualizarCliente(Cliente cli)
        {            
            string mensaje = "";
            using (SqlConnection cn = new conexion().getcn)
            {
                cn.Open();
                SqlTransaction tst = cn.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                                        
                    SqlCommand cmd = new SqlCommand("usp_actualizar_cliente", cn, tst);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", cli.idCliente);
                    cmd.Parameters.AddWithValue("@nombre", cli.nombreCliente);
                    cmd.Parameters.AddWithValue("@direccion", cli.direccion);
                    cmd.Parameters.AddWithValue("@pais", cli.idPais);
                    cmd.Parameters.AddWithValue("@telefono", cli.telefono);
                    cmd.ExecuteNonQuery();
                                        
                    tst.Commit();
                    mensaje = $"Se ha actualizado los datos del Cliente '{cli.nombreCliente}'";

                }
                catch (Exception ex)
                {
                    mensaje = $"No se pudo Actualizar los datos del Cliente '{cli.nombreCliente}'";
                    tst.Rollback();
                }
                finally { cn.Close(); }

            }
            ViewBag.mensaje = mensaje;

            return View(await Task.Run(() => cli));
        }

        
        public async Task<IActionResult> EliminarCliente(String id)
        {
            Cliente cli = BuscarCliente(id);

            if (cli == null)
            {
                return RedirectToAction("ListarClientes");
            }
            else
            {
                ViewBag.paises = new SelectList(ListarPaises(), "idPais", "nombrePais");
                return View(await Task.Run(() => cli));
            }


        }

        [HttpPost]
        public async Task<IActionResult> EliminarCliente(Cliente cli)
        {            
            string mensaje = "";
            using (SqlConnection cn = new conexion().getcn)
            {
                cn.Open();
                SqlTransaction tst = cn.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                    
                    SqlCommand cmd = new SqlCommand("exec usp_eliminar_cliente @id", cn, tst);
                    //cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", cli.idCliente);
                    cmd.ExecuteNonQuery();
                    
                    //Si todo está ok
                    tst.Commit();
                    mensaje = $"Se ha eliminado al Cliente '{cli.nombreCliente}'";

                }
                catch (Exception ex)
                {
                    mensaje = ex.Message;
                    tst.Rollback();
                }
                finally { cn.Close(); }

            }
            ViewBag.mensaje = mensaje;

            return View(await Task.Run(() => cli));
        }

        
        //REPORTES
        IEnumerable<Cliente> listarClientesXNombre(String nombre)
        {
            List<Cliente> listCliente = new List<Cliente>();
            using (SqlConnection cn = new conexion().getcn)
            {

                SqlCommand cmd = new SqlCommand("exec usp_listar_clientesXnombre @nombre", cn);
                cmd.Parameters.AddWithValue("@nombre", nombre);                
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    listCliente.Add(new Cliente()
                    {
                        idCliente = dr.GetString(0),
                        nombreCliente = dr.GetString(1),
                        direccion = dr.GetString(2),
                        idPais = dr.GetInt32(3),
                        telefono = dr.GetString(4)
                    });
                }
                dr.Close(); cn.Close();

                if(listCliente.Count==0)
                {
                    ViewBag.mensaje = $"Cliente '{nombre}' no encontrado";
                }
            }
            return listCliente;
        }

        public async Task<IActionResult> ReporteClientesXNombre(String? nombre = "")
        {
            String nombreBuscar = (nombre == null ? "" : nombre);
            ViewBag.nombreCli = nombre;
            
            IEnumerable<Cliente> reporte = listarClientesXNombre(nombreBuscar);                     

            return View(await Task.Run(() => reporte));
        }




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

        public async Task<ActionResult> RegistrarProveedor()
        {
            ViewBag.paises = new SelectList(await Task.Run(() => paises()), "idpais", "nombrepais");
            return View(new Proveedor());
        }

        [HttpPost]public async Task<IActionResult> RegistrarProveedor(Proveedor model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.paises = new SelectList(await Task.Run(() => paises()), "idpais", "nombrepais",model.idPais);
                return View(model);
            }
            string mensaje = "";
            using (SqlConnection cn = new conexion().getcn)
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_agregar_proveedor", cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idProveedor", model.idProv);
                    cmd.Parameters.AddWithValue("@nombre", model.nombreProv);
                    cmd.Parameters.AddWithValue("@direccion ", model.direccion);
                    cmd.Parameters.AddWithValue("@pais", model.idPais);
                    cmd.Parameters.AddWithValue("@telefono ", model.telefono);
                    cmd.Parameters.AddWithValue("@correo ", model.telefono);
                    cn.Open();
                    int c = cmd.ExecuteNonQuery();
                    mensaje = $"Proveedor {c} agregado";


                }
                catch (Exception ex) { mensaje = ex.Message; }
            }
            ViewBag.mensaje = mensaje;
            ViewBag.paises = new SelectList(await Task.Run(() => paises()), "idpais", "nombrepais", model.idPais);
            return View(model);

        }


        //
        //




        //REPORTE DE PROVEEDORES (listado por consulta)



        /* ---------------------------  LADY  ---------------------------*/
        //MANTENIMIENTO DE EMPLEADOS (formulario y listado)

        public IEnumerable<Empleado> ListadoEmpleados()
        {
            List<Empleado> temporal = new List<Empleado>();
            using (SqlConnection cn = new conexion().getcn)
            {
                SqlCommand cmd = new SqlCommand("exec usp_listar_empleados", cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    temporal.Add(new Empleado()
                    {
                        idEmpleado = dr.GetInt32(0),
                        apeEmpleado = dr.GetString(1),
                        nomEmpleado = dr.GetString(2),
                        fecNac = dr.GetDateTime(3),
                        fecCon = dr.GetDateTime(4),
                        correo = dr.GetString(5),
                        clave = dr.GetString(6)
                    });
                }
            }
            return temporal;
        }

        public async Task<IActionResult> ListarEmpleados()
        {
            IEnumerable<Empleado> temporal = ListadoEmpleados();
            
            return View(await Task.Run(() => temporal));
        }

        Empleado BuscarEmp(int id)
        {
            String codigo = id.ToString();

            if (string.IsNullOrEmpty(codigo))
                return new Empleado();
            else
                return ListadoEmpleados().FirstOrDefault(c => c.idEmpleado == id);

        }

        public async Task<IActionResult> LoginRegistrarEmpleado(int id)
        {
            Empleado emp = BuscarEmp(id);

            return View(await Task.Run(() => emp));
        }

        [HttpPost]
        public async Task<IActionResult> LoginRegistrarEmpleado(Empleado emp)
        {
            int c = 0;
            string mensaje = "";
            using (SqlConnection cn = new conexion().getcn)
            {
                cn.Open();
                SqlTransaction tr = cn.BeginTransaction(IsolationLevel.Serializable);
                try
                {

                    //Agrega empleado
                    SqlCommand cmd = new SqlCommand("usp_agregar_empleado", cn, tr);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@apellido", emp.apeEmpleado);
                    cmd.Parameters.AddWithValue("@nombre", emp.nomEmpleado);
                    cmd.Parameters.AddWithValue("@fecnac", emp.fecNac);
                    c = cmd.ExecuteNonQuery();


                    if (c != 0)
                        //Crea un usuario con el empleado
                        cmd = new SqlCommand("exec usp_agregar_usuario @correo, @clave", cn, tr);
                    cmd.Parameters.AddWithValue("@correo", emp.correo);
                    cmd.Parameters.AddWithValue("@clave", emp.clave);
                    cmd.ExecuteNonQuery();

                    //Si todo está ok
                    tr.Commit();
                    mensaje = "Se ha registrado con éxito";

                }
                catch (Exception ex)
                {
                    mensaje = "Ya existe empleado con este correo.";
                    tr.Rollback();
                }
                finally { cn.Close(); }

            }

            ViewBag.mensaje = mensaje;

            return View(await Task.Run(() => emp));

        }

        //Registro de empleados
        public async Task<IActionResult> RegistrarEmpleado(int id)
        {
            Empleado emp = BuscarEmp(id);

            return View(await Task.Run(() => emp));
            
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarEmpleado(Empleado emp)
        {
            

            int c = 0;
            string mensaje = "";
            using (SqlConnection cn = new conexion().getcn)
            {
                cn.Open();
                SqlTransaction tr = cn.BeginTransaction(IsolationLevel.Serializable);
                try
                {

                    //Agrega empleado
                    SqlCommand cmd = new SqlCommand("usp_agregar_empleado", cn, tr);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@apellido", emp.apeEmpleado);
                    cmd.Parameters.AddWithValue("@nombre", emp.nomEmpleado);
                    cmd.Parameters.AddWithValue("@fecnac", emp.fecNac);
                    c = cmd.ExecuteNonQuery();


                    if (c != 0 ) 
                    //Crea un usuario con el empleado
                    cmd = new SqlCommand("exec usp_agregar_usuario @correo, @clave", cn, tr);
                    cmd.Parameters.AddWithValue("@correo", emp.correo);
                    cmd.Parameters.AddWithValue("@clave", emp.clave);
                    cmd.ExecuteNonQuery();
                    
                    //Si todo está ok
                    tr.Commit();
                    mensaje = "Se ha registrado con éxito";

                }
                catch (Exception ex)
                {
                    mensaje = ex.Message;
                    tr.Rollback();
                }
                finally { cn.Close(); }

            }

            ViewBag.mensaje = mensaje;

            return View(await Task.Run(() => emp));

        }

        public async Task<IActionResult> ActualizarEmpleado(int id)
        {
            Empleado emp = BuscarEmp(id);

            if (emp == null)
            {
                return RedirectToAction("ListarEmpleados");
            }
            else
            {
                return View(emp);
            }
            return View(await Task.Run(() => emp));

        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEmpleado(Empleado emp)
        {

            int c = 0;
            string mensaje = "";
            using (SqlConnection cn = new conexion().getcn)
            {
                cn.Open();
                SqlTransaction tr = cn.BeginTransaction(IsolationLevel.Serializable);
                try
                {

                    //Agrega empleado
                    SqlCommand cmd = new SqlCommand("usp_actualizar_empleado", cn, tr);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", emp.idEmpleado);
                    cmd.Parameters.AddWithValue("@apellido", emp.apeEmpleado);
                    cmd.Parameters.AddWithValue("@nombre", emp.nomEmpleado);
                    cmd.Parameters.AddWithValue("@fecnac", emp.fecNac);
                    c = cmd.ExecuteNonQuery();

                    if (c != 0)
                        //Crea un usuario con el empleado
                        cmd = new SqlCommand("exec usp_actualizar_usuario @id, @correo, @clave", cn, tr);
                    cmd.Parameters.AddWithValue("@id", emp.idEmpleado);
                    cmd.Parameters.AddWithValue("@correo", emp.correo);
                    cmd.Parameters.AddWithValue("@clave", emp.clave);
                    cmd.ExecuteNonQuery();

                    //Si todo está ok
                    tr.Commit();
                    mensaje = "Se ha actualizado con éxito";

                }
                catch (Exception ex)
                {
                    mensaje = ex.Message;
                    tr.Rollback();
                }
                finally { cn.Close(); }

            }
            ViewBag.mensaje = mensaje;

            return View(await Task.Run(() => emp));
        }

        //B
        public async Task<IActionResult> EliminarEmpleado(int id)
        {
            Empleado emp = BuscarEmp(id);

            if (emp == null)
            {
                return RedirectToAction("ListarEmpleados");
            }
            else
            {
                return View(await Task.Run(() => emp));
            }


        }

        [HttpPost]
        public async Task<IActionResult> EliminarEmpleado(Empleado emp)
        {
            int c = 0;
            string mensaje = "";
            using (SqlConnection cn = new conexion().getcn)
            {
                cn.Open();
                SqlTransaction tr = cn.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                    
                    SqlCommand cmd = new SqlCommand("usp_eliminar_empleado", cn, tr);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ide", emp.idEmpleado);
                    c = cmd.ExecuteNonQuery();

                    //Si todo está ok
                    tr.Commit();
                    mensaje = "Se ha eliminado correctamente.";
                    

                }
                catch (Exception ex)
                {
                    mensaje = ex.Message;
                    tr.Rollback();
                }
                finally { cn.Close(); }

            }
            ViewBag.mensaje = mensaje;

            return View(await Task.Run(() => emp));
        }

        //C

        //REPORTE DE EMPLEADOS
        IEnumerable<Empleado> listaEmpleadosXFechas(DateTime f1, DateTime f2)
        {
            
            List<Empleado> temporal = new List<Empleado>();
            using (SqlConnection cn = new conexion().getcn)
            {

                SqlCommand cmd = new SqlCommand("exec usp_listar_empleadosXfecha @f1, @f2", cn);
                cmd.Parameters.AddWithValue("@f1", f1);
                cmd.Parameters.AddWithValue("@f2", f2);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    temporal.Add(new Empleado()
                    {
                        idEmpleado = dr.GetInt32(0),
                        apeEmpleado = dr.GetString(1),
                        nomEmpleado = dr.GetString(2),
                        fecNac = dr.GetDateTime(3),
                        fecCon = dr.GetDateTime(4)
                    });
                }
                dr.Close(); cn.Close();
            }
            return temporal;
        }

        public async Task<IActionResult> consultaEmpleadosXFechas(/*int p = 0,*/ DateTime? f1 = null, DateTime? f2 = null)
        {


            DateTime x1 = (f1 == null ? DateTime.Today.AddDays(1) : (DateTime)f1);
            DateTime x2 = (f2 == null ? DateTime.Today.AddDays(1) : (DateTime)f2);

            IEnumerable<Empleado> temporal = listaEmpleadosXFechas(x1, x2);
            /*
            int f = 10;
            int c = temporal.Count();
            int npags = c % f == 0 ? c / f : c / f + 1;
            
            ViewBag.npags = npags;
            ViewBag.p = p;
            ViewBag.f1 = f1;
            ViewBag.f2 = f2;
            */

            //return View(await Task.Run(() => temporal.Skip(f * p).Take(f)));
            return View(await Task.Run(() => temporal));
        }

    }
}
