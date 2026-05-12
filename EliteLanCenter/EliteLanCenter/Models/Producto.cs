// =============================================
// ELITE LAN CENTER - MODELO DE PRODUCTO
// =============================================

using System.Windows.Media;

namespace EliteLanCenter.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public double PrecioUnidad { get; set; }
        public double PrecioPaquete { get; set; }
        public int UnidadesPaquete { get; set; }
        public bool Activo { get; set; } = true;
        public string CreadoEn { get; set; } = string.Empty;

        public int StockMostrador { get; set; }
        public int PaquetesAlmacen { get; set; }
        public int UnidadesSueltas { get; set; }

        public int TotalUnidadesAlmacen =>
            (PaquetesAlmacen * UnidadesPaquete) + UnidadesSueltas;

        public double ValorMostrador =>
            StockMostrador * PrecioUnidad;

        public double ValorAlmacen =>
            TotalUnidadesAlmacen * PrecioUnidad;

        public double ValorTotal =>
            ValorMostrador + ValorAlmacen;

        public string PrecioUnidadFormateado =>
            $"S/ {PrecioUnidad:F2}";

        public string PrecioPaqueteFormateado =>
            $"S/ {PrecioPaquete:F2}";

        public string ValorTotalFormateado =>
            $"S/ {ValorTotal:F2}";

        public Brush StockAlertaColor { get; set; } = Brushes.Transparent;
        public Brush StockAlertaTexto { get; set; } = Brushes.White;
    }
}