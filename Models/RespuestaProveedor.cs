namespace Proyecto_compras.Models
{
    public class RespuestaProveedor
    {
        public int Id { get; set; }
        public int SolicitudId { get; set; }
        public int ProveedorId { get; set; }
        public string NumSolicitud { get; set; }

        // Datos de los artículos cotizados
        public string NumArticulo { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioTotal { get; set; }

        // Datos adicionales
        public string MetodoPago { get; set; }
        public string TiempoEntrega { get; set; }

        public DateTime FechaRespuesta { get; set; } = DateTime.Now;
    }
}
