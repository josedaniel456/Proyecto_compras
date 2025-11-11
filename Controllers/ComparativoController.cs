using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Proyecto_compras.Data;
using Proyecto_compras.Models;

namespace Proyecto_compras.Controllers
{
    public class ComparativoController : Controller
    {
        private readonly ConexionBD _conexionBD;

        public ComparativoController(ConexionBD conexionBD)
        {
            _conexionBD = conexionBD;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string numSolicitud)
        {
            var model = new ComparativoViewModel
            {
                NumSolicitud = numSolicitud,
                Proveedores = new List<ProveedorComparativo>()
            };

            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();

                // 🔹 Obtener el ID de la solicitud según el número (ej. SOL-001)
                string queryId = "SELECT id FROM solicitud WHERE num_solicitud = @numSolicitud";
                int solicitudId = 0;
                using (var cmd = new MySqlCommand(queryId, conexion))
                {
                    cmd.Parameters.AddWithValue("@numSolicitud", numSolicitud);
                    var result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        ViewBag.Error = "No se encontró una solicitud con ese número.";
                        return View();
                    }
                    solicitudId = Convert.ToInt32(result);
                }

                // 🔹 Obtener todas las respuestas de los proveedores para esa solicitud
                string query = @"
                    SELECT p.razon_social, r.num_articulo, r.cantidad, r.precio_unitario, r.precio_total
                    FROM respuesta_proveedor r
                    INNER JOIN proveedores p ON p.id = r.proveedor_id
                    WHERE r.solicitud_id = @solicitud_id
                    ORDER BY p.razon_social";

                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@solicitud_id", solicitudId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        var proveedores = new Dictionary<string, ProveedorComparativo>();

                        while (reader.Read())
                        {
                            string razonSocial = reader["razon_social"].ToString();

                            if (!proveedores.ContainsKey(razonSocial))
                            {
                                proveedores[razonSocial] = new ProveedorComparativo
                                {
                                    RazonSocial = razonSocial,
                                    Items = new List<ItemComparativo>()
                                };
                            }

                            proveedores[razonSocial].Items.Add(new ItemComparativo
                            {
                                NumArticulo = reader["num_articulo"].ToString(),
                                Cantidad = Convert.ToInt32(reader["cantidad"]),
                                PrecioUnitario = Convert.ToDecimal(reader["precio_unitario"]),
                                PrecioTotal = Convert.ToDecimal(reader["precio_total"])
                            });
                        }

                        model.Proveedores = proveedores.Values.ToList();
                    }
                }

                // 🔹 Calcular total general por proveedor
                foreach (var prov in model.Proveedores)
                {
                    prov.TotalGeneral = prov.Items.Sum(i => i.PrecioTotal);
                }
            }

            return View(model);
        }
    }
}
