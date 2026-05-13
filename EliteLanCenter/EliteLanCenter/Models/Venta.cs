// =============================================
// ELITE LAN CENTER - MODELO DE VENTA
// =============================================

namespace EliteLanCenter.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public int TurnoId { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double PrecioUnit { get; set; }
        public double Total { get; set; }
        public bool Fiado { get; set; } = false;
        public bool FiadoPagado { get; set; } = false;
        public string? NumeroPc { get; set; }
        public string CreadoEn { get; set; } = string.Empty;

        // Propiedades calculadas
        public string TotalFormateado =>
            $"S/ {Total:F2}";

        public string PrecioUnitFormateado =>
            $"S/ {PrecioUnit:F2}";

        public string EstadoFiado =>
            Fiado && !FiadoPagado ? "⏳ Pendiente" :
            Fiado && FiadoPagado ? "✅ Pagado" : "—";

        public string HoraVenta
        {
            get
            {
                if (DateTime.TryParse(CreadoEn, out var hora))
                    return hora.ToString("hh:mm tt");
                return CreadoEn;
            }
        }

        public bool EsFiadoPendiente => Fiado && !FiadoPagado;

        public string NumeroPcDisplay =>
            string.IsNullOrWhiteSpace(NumeroPc) ? "" : $"PC #{NumeroPc}";
    }
}