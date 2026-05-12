// =============================================
// ELITE LAN CENTER - TURNOS PAGE
// =============================================

using EliteLanCenter.Controllers;
using EliteLanCenter.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EliteLanCenter.Pages
{
    public partial class TurnosPage : Page
    {
        private readonly Usuario _usuario;
        private Turno? _turnoActivo;

        public TurnosPage(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            CargarDatos();
        }

        // ── Cargar datos ───────────────────────
        private void CargarDatos()
        {
            _turnoActivo = TurnoController.ObtenerTurnoAbierto();

            if (_turnoActivo != null)
            {
                LblTurnoActivo.Text = $"✅ {_turnoActivo.TipoDescripcion}";
                LblTurnoActivo.Foreground = new SolidColorBrush(
                    Color.FromRgb(0x01, 0xEF, 0xAC));
                LblInfoTurno.Text = $"👤 {_turnoActivo.NombreOperador}  |  📅 {_turnoActivo.FechaFormateada}  |  🕐 {_turnoActivo.HoraApertura}";
                BtnCerrarTurno.Visibility = Visibility.Visible;
            }
            else
            {
                LblTurnoActivo.Text = "⚠ No hay turno abierto";
                LblTurnoActivo.Foreground = new SolidColorBrush(
                    Color.FromRgb(0xF5, 0x9E, 0x0B));
                LblInfoTurno.Text = "—";
                BtnCerrarTurno.Visibility = Visibility.Collapsed;
            }

            // Cargar stock mostrador
            CargarStockMostrador();

            // Cargar semana actual
            CargarSemana();
        }

        // ── Stock mostrador ────────────────────
        private void CargarStockMostrador()
        {
            var productos = ProductoController.ObtenerTodos();
            ListaStock.ItemsSource = productos;

            var total = productos.Sum(p => p.ValorMostrador);
            LblTotalMostrador.Text = $"S/ {total:F2}";
        }

        // ── Semana actual ──────────────────────
        private void CargarSemana()
        {
            var turnos = TurnoController.ObtenerTurnosSemanaActual();
            var hoy = DateTime.Now.Date;
            var lunes = hoy.AddDays(-(int)hoy.DayOfWeek + (int)DayOfWeek.Monday);

            var diasSemana = new List<DiaSemanaViewModel>();

            var nombresDias = new[] { "Lunes", "Martes", "Miércoles",
                                      "Jueves", "Viernes", "Sábado", "Domingo" };

            for (int i = 0; i < 7; i++)
            {
                var fecha = lunes.AddDays(i);
                var fechaStr = fecha.ToString("yyyy-MM-dd");
                var fechaLabel = fecha.ToString("dd/MM");

                var turnoManana = turnos.FirstOrDefault(t =>
                    t.Fecha == fechaStr && t.Tipo == "Mañana");
                var turnoTarde = turnos.FirstOrDefault(t =>
                    t.Fecha == fechaStr && t.Tipo == "Tarde");
                var turnoNoche = turnos.FirstOrDefault(t =>
                    t.Fecha == fechaStr && t.Tipo == "Noche");

                diasSemana.Add(new DiaSemanaViewModel
                {
                    DiaNombre = $"{(fecha.Date == hoy ? "📍 " : "")}{nombresDias[i]}",
                    Fecha = fechaLabel,
                    EstadoManana = turnoManana == null ? "— —" :
                                   turnoManana.Abierto ? "🔄 Abierto" : "✅ Cerrado",
                    EstadoTarde = turnoTarde == null ? "— —" :
                                   turnoTarde.Abierto ? "🔄 Abierto" : "✅ Cerrado",
                    EstadoNoche = turnoNoche == null ? "— —" :
                                   turnoNoche.Abierto ? "🔄 Abierto" : "✅ Cerrado",
                    ColorManana = turnoManana == null ? "#2D3250" :
                                   turnoManana.Abierto ? "#F59E0B" : "#01EFAC",
                    ColorTarde = turnoTarde == null ? "#2D3250" :
                                   turnoTarde.Abierto ? "#F59E0B" : "#01EFAC",
                    ColorNoche = turnoNoche == null ? "#2D3250" :
                                   turnoNoche.Abierto ? "#F59E0B" : "#01EFAC",
                });
            }

            ListaSemana.ItemsSource = diasSemana;
        }

        // ── Cerrar turno ───────────────────────
        private void BtnCerrarTurno_Click(object sender, RoutedEventArgs e)
        {
            if (_turnoActivo == null) return;

            var confirmar = MessageBox.Show(
                "¿Estás seguro que deseas cerrar el turno?\nEsto registrará el conteo para el siguiente trabajador.",
                "Cerrar Turno",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmar == MessageBoxResult.Yes)
            {
                var (ok, mensaje) = TurnoController.CerrarTurno(_turnoActivo.Id);

                if (ok)
                {
                    MessageBox.Show(mensaje, "✅ Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarDatos();
                }
                else
                {
                    MessageBox.Show(mensaje, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

    // ── ViewModel para la semana ───────────────
    public class DiaSemanaViewModel
    {
        public string DiaNombre { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string EstadoManana { get; set; } = "— —";
        public string EstadoTarde { get; set; } = "— —";
        public string EstadoNoche { get; set; } = "— —";
        public string ColorManana { get; set; } = "#2D3250";
        public string ColorTarde { get; set; } = "#2D3250";
        public string ColorNoche { get; set; } = "#2D3250";
    }
}