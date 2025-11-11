using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Proyecto_compras.Data;
using Proyecto_compras.Models;

namespace Proyecto_compras.Controllers
{
    public class ProveedorRespuestaController : Controller
    {
        private readonly ConexionBD _conexionBD;

        public ProveedorRespuestaController(ConexionBD conexionBD)
        {
            _conexionBD = conexionBD;
        }

        [HttpGet]
        public IActionResult Form(string numSolicitud, int proveedorId)
        {
            // Mostrar datos básicos en el encabezado
            ViewBag.NumSolicitud = numSolicitud;
            ViewBag.ProveedorId = proveedorId;

            return View();
        }

        [HttpPost]
        public IActionResult Form(RespuestaProveedor model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();

                // Buscar el ID de la solicitud con el número
                string querySolicitud = "SELECT id FROM solicitud WHERE num_solicitud = @num";
                int solicitudId = 0;
                using (var cmd = new MySqlCommand(querySolicitud, conexion))
                {
                    cmd.Parameters.AddWithValue("@num", model.NumSolicitud);
                    var result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        ViewBag.Error = "No se encontró la solicitud.";
                        return View(model);
                    }
                    solicitudId = Convert.ToInt32(result);
                }

                // Insertar la respuesta del proveedor
                string insert = @"INSERT INTO respuesta_proveedor 
                    (solicitud_id, proveedor_id, num_articulo, cantidad, precio_unitario, precio_total, metodo_pago, tiempo_entrega, fecha_respuesta)
                    VALUES (@solicitud_id, @proveedor_id, @num_articulo, @cantidad, @precio_unitario, @precio_total, @metodo_pago, @tiempo_entrega, NOW())";

                using (var cmd = new MySqlCommand(insert, conexion))
                {
                    cmd.Parameters.AddWithValue("@solicitud_id", solicitudId);
                    cmd.Parameters.AddWithValue("@proveedor_id", model.ProveedorId);
                    cmd.Parameters.AddWithValue("@num_articulo", model.NumArticulo);
                    cmd.Parameters.AddWithValue("@cantidad", model.Cantidad);
                    cmd.Parameters.AddWithValue("@precio_unitario", model.PrecioUnitario);
                    cmd.Parameters.AddWithValue("@precio_total", model.Cantidad * model.PrecioUnitario);
                    cmd.Parameters.AddWithValue("@metodo_pago", model.MetodoPago);
                    cmd.Parameters.AddWithValue("@tiempo_entrega", model.TiempoEntrega);
                    cmd.ExecuteNonQuery();
                }
            }

            ViewBag.Exito = "Tu cotización fue enviada correctamente.";
            ModelState.Clear();
            return View();

        }
    }
}
