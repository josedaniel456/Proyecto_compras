namespace Proyecto_compras.Models
{
    public class Solicitud
    {
        public int Id { get; set; }
        public string NumReque { get; set; }
        public string NumSolicitud { get; set; }

        public string Planta { get; set; }
        public DateTime FechaEntrega { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; }
        public List<int> ProveedoresIds { get; set; } = new List<int>();

    }
}
