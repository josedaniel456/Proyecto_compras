using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Proyecto_compras.Data;
using Proyecto_compras.Models;
using System.Net;
using System.Net.Mail;

namespace Proyecto_compras.Controllers
{
    public class SolicitudController : Controller
    {
        private readonly ConexionBD _conexionBD;

        public SolicitudController(ConexionBD conexionBD)
        {
            _conexionBD = conexionBD;
        }

        public IActionResult Crear()
        {
            var proveedores = new List<Proveedor>();

            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();
                string query = "SELECT Id, razon_social, correo FROM proveedores WHERE estado = 1";
                using (var cmd = new MySqlCommand(query, conexion))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        proveedores.Add(new Proveedor
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            razon_social = reader["razon_social"].ToString(),
                            correo = reader["correo"].ToString()
                        });
                    }
                }
            }

            ViewBag.Proveedores = proveedores;
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Solicitud solicitud)
        {
            solicitud.FechaCreacion = DateTime.Now;
            solicitud.Estado = "Pendiente";

            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();

                // ✅ Generar número de solicitud tipo SOL-001, SOL-002...
                string nuevoNumero = GenerarNumeroSolicitud(conexion);

                // ✅ Insertar solicitud
                string query = @"INSERT INTO solicitud (num_solicitud, num_reque, planta, fecha_entrega, fecha_creacion, estado)
                                 VALUES (@num_solicitud, @num_reque, @planta, @fecha_entrega, @fecha_creacion, @estado);
                                 SELECT LAST_INSERT_ID();";

                int solicitudId;
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@num_solicitud", nuevoNumero);
                    cmd.Parameters.AddWithValue("@num_reque", solicitud.NumReque);
                    cmd.Parameters.AddWithValue("@planta", solicitud.Planta);
                    cmd.Parameters.AddWithValue("@fecha_entrega", solicitud.FechaEntrega);
                    cmd.Parameters.AddWithValue("@fecha_creacion", solicitud.FechaCreacion);
                    cmd.Parameters.AddWithValue("@estado", solicitud.Estado);

                    solicitudId = Convert.ToInt32(cmd.ExecuteScalar());
                }
                var correoUsuario = HttpContext.Session.GetString("correoUsuario");
                var nombreUsuario = HttpContext.Session.GetString("nombreUsuario");
                if (!string.IsNullOrEmpty(correoUsuario))
                {
                    EnviarCorreoUsuario(correoUsuario, nombreUsuario, nuevoNumero);
                }

                // ✅ Insertar relación con proveedores y enviar correo
                foreach (var proveedorId in solicitud.ProveedoresIds)
                {
                    string insertRelacion = @"INSERT INTO solicitud_proveedor (solicitud_id, proveedor_id)
                                              VALUES (@solicitud_id, @proveedor_id)";
                    using (var cmdRel = new MySqlCommand(insertRelacion, conexion))
                    {
                        cmdRel.Parameters.AddWithValue("@solicitud_id", solicitudId);
                        cmdRel.Parameters.AddWithValue("@proveedor_id", proveedorId);
                        cmdRel.ExecuteNonQuery();
                    }

                    // ✅ Ahora sí, pasamos el número de solicitud
                    EnviarCorreoProveedor(proveedorId, nuevoNumero);
                }
            }

            TempData["Mensaje"] = "Solicitud enviada correctamente.";
            return RedirectToAction("Crear");
        }
        private void EnviarCorreoUsuario(string correoUsuario, string nombreUsuario, string numSolicitud)
        {
            try
            {
                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("joseBlox4@gmail.com", "qwil beji gmcn rrqz");
                    smtp.EnableSsl = true;

                    var mail = new MailMessage();
                    mail.From = new MailAddress("joseBlox4@gmail.com", "Sistema de Compras");
                    mail.To.Add(correoUsuario);
                    mail.Subject = $"Solicitud creada exitosamente - {numSolicitud}";
                    mail.Body =
                        $"Hola {nombreUsuario},\n\n" +
                        $"Tu solicitud ha sido registrada correctamente con el número:\n" +
                        $"➡ {numSolicitud}\n\n" +
                        $"Podrás usar este código en el módulo comparativo para revisar las cotizaciones recibidas.\n\n" +
                        $"Gracias por usar el Sistema de Compras.\n\n" +
                        $"Saludos,\nEl equipo de Compras.";

                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar correo al usuario: " + ex.Message);
            }
        }

        // ✅ Nueva función para generar el número automáticamente
        private string GenerarNumeroSolicitud(MySqlConnection conexion)
        {
            string query = "SELECT MAX(num_solicitud) FROM solicitud";
            string ultimoNumero = "SOL-000";

            using (var cmd = new MySqlCommand(query, conexion))
            {
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                    ultimoNumero = result.ToString();
            }

            // Extrae el número y genera el siguiente
            int numeroActual = int.Parse(ultimoNumero.Replace("SOL-", ""));
            string nuevoNumero = "SOL-" + (numeroActual + 1).ToString("D3");
            return nuevoNumero;
        }

        // ✅ Envío de correo con el número de solicitud correcto
        private void EnviarCorreoProveedor(int proveedorId, string numSolicitud)
        {
            string correoProveedor = "";
            string nombreProveedor = "";

            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();
                string query = "SELECT razon_social, correo FROM proveedores WHERE Id = @id";
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", proveedorId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nombreProveedor = reader["razon_social"].ToString();
                            correoProveedor = reader["correo"].ToString();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(correoProveedor))
            {
                string enlace = $"https://proyecto-compras.onrender.com/ProveedorRespuesta/Form?numSolicitud={numSolicitud}&proveedorId={proveedorId}";

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("joseBlox4@gmail.com", "qwil beji gmcn rrqz");
                    smtp.EnableSsl = true;

                    var mail = new MailMessage();
                    mail.From = new MailAddress("joseBlox4@gmail.com", "Sistema de Compras");
                    mail.To.Add(correoProveedor);
                    mail.Subject = $"Nueva Solicitud #{numSolicitud}";
                    mail.Body =
                        $"Estimado {nombreProveedor},\n\n" +
                        $"Ha recibido una nueva solicitud de cotización con el número {numSolicitud}.\n" +
                        $"Por favor ingrese sus precios y condiciones en el siguiente enlace:\n\n" +
                        $"{enlace}\n\n" +
                        $"Saludos,\nSistema de Compras";

                    smtp.Send(mail);
                }
            }
        }
    }
}
