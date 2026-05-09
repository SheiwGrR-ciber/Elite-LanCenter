// =============================================
// ELITE LAN CENTER - DASHBOARD PAGE
// =============================================

using EliteLanCenter.Controllers;
using EliteLanCenter.Models;
using System.Windows;
using System.Windows.Controls;

namespace EliteLanCenter.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly Usuario _usuario;
        private Turno? _turnoActivo;

        public DashboardPage(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            CargarDatos();
            SugerirTurno();
        }

        // ── Cargar datos del dashboard ─────────
        private void CargarDatos()
        {
            // Turno activo
            _turnoActivo = TurnoController.ObtenerTurnoAbierto();

            if (_turnoActivo != null)
            {
                // Mostrar turno activo
                LblEstadoTurno.Text = $"✅ {_turnoActivo.TipoDescripcion} abierto";
                LblEstadoTurno.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x01, 0xEF, 0xAC));
                LblInfoTurno.Text = $"👤 {_turnoActivo.NombreOperador}  |  📅 {_turnoActivo.FechaFormateada}  |  🕐 Apertura: {_turnoActivo.HoraApertura}";
                LblTurnoActivo.Text = _turnoActivo.Tipo;

                // Mostrar botón cerrar turno
                PanelAbrirTurno.Visibility = Visibility.Collapsed;
                BtnCerrarTurno.Visibility = Visibility.Visible;

                // Total ventas
                var totalVentas = VentaController.ObtenerTotalTurno(_turnoActivo.Id);
                LblVentasTurno.Text = $"S/ {totalVentas:F2}";
            }
            else
            {
                LblEstadoTurno.Text = "⚠ No hay turno abierto";
                LblEstadoTurno.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0xF5, 0x9E, 0x0B));
                LblInfoTurno.Text = "Abre un turno para comenzar";
                LblTurnoActivo.Text = "Ninguno";

                PanelAbrirTurno.Visibility = Visibility.Visible;
                BtnCerrarTurno.Visibility = Visibility.Collapsed;
            }

            // Valor inventario
            var valorInventario = ProductoController.CalcularValorTotalInventario();
            LblValorInventario.Text = $"S/ {valorInventario:F2}";

            // Caseros activos
            var caseros = CaseroController.ObtenerTodos();
            LblCaserosActivos.Text = caseros.Count(c => c.Activo).ToString();
        }

        // ── Sugerir tipo de turno ──────────────
        private void SugerirTurno()
        {
            var sugerido = TurnoController.SugerirTipoTurno();
            foreach (ComboBoxItem item in CmbTipoTurno.Items)
            {
                if (item.Content.ToString() == sugerido)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        // ── Abrir turno ────────────────────────
        private void BtnAbrirTurno_Click(object sender, RoutedEventArgs e)
        {
            var tipo = (CmbTipoTurno.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(tipo))
            {
                MessageBox.Show("Selecciona el tipo de turno.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (ok, mensaje, _) = TurnoController.AbrirTurno(_usuario.Id, tipo);

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

        // ── Accesos rápidos ────────────────────
        private void BtnIrVentas_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Window.GetWindow(this) is Views.DashboardView dashboard)
                dashboard.NavegarA("Ventas");
        }

        private void BtnIrTurnos_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Window.GetWindow(this) is Views.DashboardView dashboard)
                dashboard.NavegarA("Turnos");
        }

        private void BtnIrReportes_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Window.GetWindow(this) is Views.DashboardView dashboard)
                dashboard.NavegarA("Reportes");
        }
    }
}