// =============================================
// ELITE LAN CENTER - MODELO DE MOVIMIENTO
// =============================================

namespace EliteLanCenter.Models
{
    public class Movimiento
    {
        public int Id { get; set; }
        public int CaseroId { get; set; }
        public string NombreCasero { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // "Recarga" o "Consumo"
        public double Monto { get; set; }
        public double SaldoAnterior { get; set; }
        public double SaldoNuevo { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string CreadoEn { get; set; } = string.Empty;

        // Propiedades calculadas
        public string MontoFormateado =>
            $"S/ {Monto:F2}";

        public string SaldoAnteriorFormateado =>
            $"S/ {SaldoAnterior:F2}";

        public string SaldoNuevoFormateado =>
            $"S/ {SaldoNuevo:F2}";

        public string TipoIcono =>
            Tipo == "Recarga" ? "💰 Recarga" : "🎮 Consumo";

        public string Hora
        {
            get
            {
                if (DateTime.TryParse(CreadoEn, out var hora))
                    return hora.ToString("hh:mm tt");
                return CreadoEn;
            }
        }

        public string FechaFormateada
        {
            get
            {
                if (DateTime.TryParse(CreadoEn, out var fecha))
                    return fecha.ToString("dd/MM/yyyy");
                return CreadoEn;
            }
        }

        public bool EsRecarga => Tipo == "Recarga";
        public bool EsConsumo => Tipo == "Consumo";
    }
}