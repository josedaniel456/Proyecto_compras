using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace Proyecto_compras.Data
{
    public class ConexionBD
    {
        private readonly string _connectionString;

        public ConexionBD(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConexionMySQL");
        }

        public MySqlConnection CrearConexion()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
