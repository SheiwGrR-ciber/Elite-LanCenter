// =============================================
// ELITE LAN CENTER - MODELO DE TURNO
// =============================================

namespace EliteLanCenter.Models
{
    public class Turno
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NombreOperador { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public bool Abierto { get; set; } = true;
        public string AbiertaEn { get; set; } = string.Empty;
        public string? CerradaEn { get; set; }

        // Propiedades calculadas
        public string TipoDescripcion => Tipo switch
        {
            "Mañana" => "🌅 Turno Mañana",
            "Tarde" => "🌤️ Turno Tarde",
            "Noche" => "🌙 Turno Noche",
            _ => Tipo
        };

        public string EstadoDescripcion =>
            Abierto ? "✅ Abierto" : "🔒 Cerrado";

        public string FechaFormateada
        {
            get
            {
                if (DateTime.TryParse(Fecha, out var fecha))
                    return fecha.ToString("dd/MM/yyyy");
                return Fecha;
            }
        }

        public string HoraApertura
        {
            get
            {
                if (DateTime.TryParse(AbiertaEn, out var hora))
                    return hora.ToString("hh:mm tt");
                return AbiertaEn;
            }
        }

        public string HoraCierre
        {
            get
            {
                if (CerradaEn != null && DateTime.TryParse(CerradaEn, out var hora))
                    return hora.ToString("hh:mm tt");
                return "—";
            }
        }
    }
}