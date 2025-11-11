using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace Proyecto_compras.Data
{
    public class ConexionBD
    {
        private readonly string _connectionString;

        public ConexionBD(IConfiguration configuration)
        {
            // ✅ Primero intenta leer del entorno (Render)
            _connectionString = Environment.GetEnvironmentVariable("DefaultConnection");

            // ✅ Si no existe, usa la conexión local de appsettings.json
            if (string.IsNullOrEmpty(_connectionString))
            {
                _connectionString = configuration.GetConnectionString("DefaultConnection");
            }
        }

        // 🔹 Cambiado de "ObtenerConexion" a "CrearConexion"
        public MySqlConnection CrearConexion()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
