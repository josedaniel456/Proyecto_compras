using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Proyecto_compras.Data;
using Proyecto_compras.Models;
using System.Security.Cryptography;
using System.Text;

namespace Proyecto_compras.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly ConexionBD _conexionBD;

        public UsuarioController(ConexionBD conexionBD)
        {
            _conexionBD = conexionBD;
        }

        public IActionResult Index()
        {
            var usuarios = new List<Usuario>();

            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();
                string query = "SELECT Id, nombre, correo, estado, rol FROM usuario";
                using (var cmd = new MySqlCommand(query, conexion))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usuarios.Add(new Usuario
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["nombre"].ToString(),
                            Correo = reader["correo"].ToString(),
                            Estado = Convert.ToBoolean(reader["estado"]),
                            Rol = reader["rol"].ToString()
                        });
                    }
                }
            }

            return View(usuarios);
        }

        // GET: Usuario/Crear
        public IActionResult Crear() => View();

        // POST: Usuario/Crear
        [HttpPost]
        public IActionResult Crear(Usuario usuario)
        {
            if (!ModelState.IsValid)
                return View(usuario);

            string passwordHash = EncriptarSHA256(usuario.Contraseña);

            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();
                string query = @"INSERT INTO usuario (nombre, correo, contraseña, estado, rol)
                                 VALUES (@nombre, @correo, @contraseña, 1, 'usuario')";
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@correo", usuario.Correo);
                    cmd.Parameters.AddWithValue("@contraseña", passwordHash);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }

        // GET: Usuario/Editar/5
        public IActionResult Editar(int id)
        {
            Usuario usuario = null;
            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();
                string query = "SELECT * FROM usuario WHERE Id = @id";
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Nombre = reader["nombre"].ToString(),
                                Correo = reader["correo"].ToString(),
                                Estado = Convert.ToBoolean(reader["estado"]),
                                Rol = reader["rol"].ToString()
                            };
                        }
                    }
                }
            }

            return View(usuario);
        }

        // POST: Usuario/Editar
        [HttpPost]
        public IActionResult Editar(Usuario usuario)
        {
            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();
                string query = "UPDATE usuario SET nombre=@nombre, correo=@correo, rol=@rol WHERE Id=@id";
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@correo", usuario.Correo);
                    cmd.Parameters.AddWithValue("@rol", usuario.Rol);
                    cmd.Parameters.AddWithValue("@id", usuario.Id);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }

        // Cambiar estado (activar/desactivar)
        public IActionResult CambiarEstado(int id, bool estado)
        {
            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();
                string query = "UPDATE usuario SET estado = @estado WHERE Id = @id";
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@estado", estado ? 1 : 0);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }

        // Encriptar contraseña SHA256
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
