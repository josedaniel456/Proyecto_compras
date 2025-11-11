namespace Proyecto_compras.Models
{
    public class ComparativoViewModel
    {
        public string NumSolicitud { get; set; }
        public List<ProveedorComparativo> Proveedores { get; set; }
    }

    public class ProveedorComparativo
    {
        public string RazonSocial { get; set; }
        public List<ItemComparativo> Items { get; set; }
        public decimal TotalGeneral { get; set; }
    }

    public class ItemComparativo
    {
        public string NumArticulo { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioTotal { get; set; }
    }
}
