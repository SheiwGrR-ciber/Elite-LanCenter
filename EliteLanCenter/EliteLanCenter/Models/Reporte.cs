// =============================================
// ELITE LAN CENTER - MODELO DE REPORTE
// =============================================

namespace EliteLanCenter.Models
{
    public class Reporte
    {
        public int Id { get; set; }
        public int TurnoId { get; set; }
        public string NombreOperador { get; set; } = string.Empty;
        public string TipoTurno { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string HoraApertura { get; set; } = string.Empty;
        public string HoraCierre { get; set; } = string.Empty;

        // Montos
        public double IngresoMaquinas { get; set; }
        public double TotalProductos { get; set; }
        public double TotalBruto { get; set; }
        public double Descuentos { get; set; }
        public double TotalLiquido { get; set; }
        public double MontoYape { get; set; }
        public double Efectivo { get; set; }

        // Imágenes de evidencia
        public string? ImagenPanCafe1 { get; set; }
        public string? ImagenPanCafe2 { get; set; }
        public string? ImagenYape { get; set; }

        // PDF generado
        public string? PdfPath { get; set; }
        public string CreadoEn { get; set; } = string.Empty;

        // Propiedades calculadas
        public string IngresoMaquinasFormateado =>
            $"S/ {IngresoMaquinas:F2}";

        public string TotalProductosFormateado =>
            $"S/ {TotalProductos:F2}";

        public string TotalBrutoFormateado =>
            $"S/ {TotalBruto:F2}";

        public string DescuentosFormateado =>
            $"S/ {Descuentos:F2}";

        public string TotalLiquidoFormateado =>
            $"S/ {TotalLiquido:F2}";

        public string MontoYapeFormateado =>
            $"S/ {MontoYape:F2}";

        public string EfectivoFormateado =>
            $"S/ {Efectivo:F2}";

        public string FechaFormateada
        {
            get
            {
                if (DateTime.TryParse(Fecha, out var fecha))
                    return fecha.ToString("dd/MM/yyyy");
                return Fecha;
            }
        }

        public bool TienePdf =>
            !string.IsNullOrEmpty(PdfPath);

        // Calcular totales automáticamente
        public void CalcularTotales()
        {
            TotalBruto = IngresoMaquinas + TotalProductos;
            TotalLiquido = TotalBruto - Descuentos;
            Efectivo = TotalLiquido - MontoYape;
        }
    }
}