// =============================================
// ELITE LAN CENTER - MODELO DE CASERO
// =============================================

namespace EliteLanCenter.Models
{
    public class Casero
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apodo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string CodigoRFID { get; set; } = string.Empty;
        public double Saldo { get; set; }
        public bool Activo { get; set; } = true;
        public string UltimaVisita { get; set; } = string.Empty;
        public string CreadoEn { get; set; } = string.Empty;

        // Propiedades calculadas
        public string SaldoFormateado =>
            $"S/ {Saldo:F2}";

        public bool SaldoBajo => Saldo < 2.00;

        public string EstadoSaldo =>
            Saldo <= 0 ? "❌ Sin saldo" :
            Saldo < 2.00 ? "⚠️ Saldo bajo" :
                              "✅ Con saldo";

        public bool EstaInactivo
        {
            get
            {
                if (DateTime.TryParse(UltimaVisita, out var ultima))
                    return (DateTime.Now - ultima).TotalDays > 30;
                return false;
            }
        }

        public string UltimaVisitaFormateada
        {
            get
            {
                if (DateTime.TryParse(UltimaVisita, out var fecha))
                    return fecha.ToString("dd/MM/yyyy");
                return "Sin visitas";
            }
        }

        public string NombreCompleto =>
            string.IsNullOrEmpty(Apodo)
                ? Nombre
                : $"{Nombre} ({Apodo})";

        public bool TieneRFID =>
            !string.IsNullOrEmpty(CodigoRFID);
    }
}