using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Proyecto_compras.Data;
using Proyecto_compras.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Proyecto_compras.Controllers
{
    public class LoginController : Controller
    {
        private readonly ConexionBD _conexionBD;
        public LoginController(ConexionBD conexionBD)
        {
            _conexionBD = conexionBD;
        }

        // Página de Login
        public IActionResult Index()
        {
            return View();
        }

        // Acción POST del login
        [HttpPost]
        public IActionResult Index(string correo, string contraseña)
        {
            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contraseña))
            {
                ViewBag.Error = "Debe ingresar correo y contraseña.";
                return View();
            }

            string passwordHash = EncriptarSHA256(contraseña);

            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();
                string query = "SELECT Id, nombre, correo, rol, estado FROM usuario WHERE correo = @correo AND contraseña = @contraseña";
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@correo", correo);
                    cmd.Parameters.AddWithValue("@contraseña", passwordHash);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool activo = Convert.ToBoolean(reader["estado"]);

                            if (!activo)
                            {
                                ViewBag.Error = "Tu cuenta está deshabilitada. Contacta con el administrador.";
                                return View();
                            }

                            // Guardar datos en sesión
                            HttpContext.Session.SetInt32("UsuarioId", Convert.ToInt32(reader["Id"]));
                            HttpContext.Session.SetString("UsuarioNombre", reader["nombre"].ToString());
                            HttpContext.Session.SetString("UsuarioCorreo", reader["correo"].ToString());
                            HttpContext.Session.SetString("UsuarioRol", reader["rol"].ToString());

                            // Redirigir según rol
                            if (reader["rol"].ToString() == "admin")
                                return RedirectToAction("Index", "Admin");
                            else
                                return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ViewBag.Error = "Correo o contraseña incorrectos.";
                        }
                    }
                }
            }

            return View();
        }

        // Logout
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // Encriptar contraseña SHA256 (idéntico al del controlador Usuario)
        private string EncriptarSHA256(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}
