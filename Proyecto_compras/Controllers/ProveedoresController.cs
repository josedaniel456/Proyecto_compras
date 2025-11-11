using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Proyecto_compras.Data;
using Proyecto_compras.Models;

namespace Proyecto_compras.Controllers
{
    public class ProveedoresController : Controller
    {
        private readonly ConexionBD _conexionBD;

        public ProveedoresController(ConexionBD conexionBD)
        {
            _conexionBD = conexionBD;
        }
        public IActionResult Index(string filtro)
        {
            var lista = new List<Proveedor>();

            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();
                string query = "SELECT * FROM proveedores";

                if (!string.IsNullOrEmpty(filtro))
                    query += " WHERE razon_social LIKE @filtro OR ruc LIKE @filtro";

                using (var cmd = new MySqlCommand(query, conexion))
                {
                    if (!string.IsNullOrEmpty(filtro))
                        cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Proveedor
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                razon_social = reader["razon_social"].ToString(),
                                ruc = reader["ruc"].ToString(),
                                correo = reader["correo"].ToString(),
                                celular = Convert.ToInt32(reader["celular"]),
                                estado = Convert.ToBoolean(reader["estado"])
                            });
                        }
                    }
                }
            }

            ViewBag.Filtro = filtro;
            return View(lista);
        }
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Proveedor p)
        {
            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();
                string query = @"INSERT INTO proveedores 
                                 (razon_social, ruc, correo, celular, estado) 
                                 VALUES (@razon, @ruc, @correo, @celular, @estado)";
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@razon", p.razon_social);
                    cmd.Parameters.AddWithValue("@ruc", p.ruc);
                    cmd.Parameters.AddWithValue("@correo", p.correo);
                    cmd.Parameters.AddWithValue("@celular", p.celular);
                    cmd.Parameters.AddWithValue("@estado", true);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult Editar(int id)
        {
            Proveedor p = null;
            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();
                string query = "SELECT * FROM proveedores WHERE Id = @id";
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            p = new Proveedor
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                razon_social = reader["razon_social"].ToString(),
                                ruc = reader["ruc"].ToString(),
                                correo = reader["correo"].ToString(),
                                celular = Convert.ToInt32(reader["celular"]),
                                estado = Convert.ToBoolean(reader["estado"])
                            };
                        }
                    }
                }
            }
            return View(p);
        }

        [HttpPost]
        public IActionResult Editar(Proveedor p)
        {
            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();
                string query = @"UPDATE proveedores 
                                 SET razon_social=@razon, ruc=@ruc, correo=@correo, celular=@celular 
                                 WHERE Id=@id";
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@razon", p.razon_social);
                    cmd.Parameters.AddWithValue("@ruc", p.ruc);
                    cmd.Parameters.AddWithValue("@correo", p.correo);
                    cmd.Parameters.AddWithValue("@celular", p.celular);
                    cmd.Parameters.AddWithValue("@id", p.Id);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult CambiarEstado(int id)
        {
            using (var conexion = _conexionBD.CrearConexion())
            {
                conexion.Open();
                string query = "UPDATE proveedores SET estado = NOT estado WHERE Id = @id";
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
    }
}