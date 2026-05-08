// =============================================
// ELITE LAN CENTER - DASHBOARD VIEW CODE BEHIND
// =============================================

using EliteLanCenter.Controllers;
using EliteLanCenter.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace EliteLanCenter.Views
{
    public partial class DashboardView : Window
    {
        private readonly Usuario _usuario;
        private readonly DispatcherTimer _reloj;

        public DashboardView(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;

            // Configurar reloj
            _reloj = new DispatcherTimer();
            _reloj.Interval = TimeSpan.FromSeconds(1);
            _reloj.Tick += Reloj_Tick;
            _reloj.Start();

            // Inicializar base de datos
            Database.DatabaseConnection.Initialize();
            Database.DatabaseConnection.InsertarDatosIniciales();

            // Configurar UI
            ConfigurarUsuario();
            ActualizarFechaHora();

            // Cargar página inicial
            NavegaryA("Dashboard");
        }

        // ── Configurar usuario ─────────────────
        private void ConfigurarUsuario()
        {
            LblNombreUsuario.Text = _usuario.Nombre;
            LblRolUsuario.Text = _usuario.RolDescripcion;

            // Mostrar opciones de admin
            if (_usuario.EsAdmin)
            {
                LblMenuAdmin.Visibility = Visibility.Visible;
                BtnInventario.Visibility = Visibility.Visible;
                BtnAdministracion.Visibility = Visibility.Visible;
                BtnEstadisticas.Visibility = Visibility.Visible;
            }
        }

        // ── Reloj ──────────────────────────────
        private void Reloj_Tick(object? sender, EventArgs e)
        {
            ActualizarFechaHora();
        }

        private void ActualizarFechaHora()
        {
            LblFecha.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy",
                            new System.Globalization.CultureInfo("es-PE"));
            LblHora.Text = DateTime.Now.ToString("hh:mm:ss tt");
        }

        // ── Navegación ─────────────────────────
        private void NavegaryA(string pagina)
        {
            LblTituloPagina.Text = pagina;

            switch (pagina)
            {
                case "Dashboard":
                    MainFrame.Navigate(new Pages.DashboardPage(_usuario));
                    break;
                case "Ventas":
                    MainFrame.Navigate(new Pages.VentasPage(_usuario));
                    break;
                case "Turnos":
                    MainFrame.Navigate(new Pages.TurnosPage(_usuario));
                    break;
                case "Reportes":
                    MainFrame.Navigate(new Pages.ReportesPage(_usuario));
                    break;
                case "Caseros":
                    MainFrame.Navigate(new Pages.CaserosPage(_usuario));
                    break;
                case "Inventario":
                    MainFrame.Navigate(new Pages.InventarioPage(_usuario));
                    break;
                case "Administracion":
                    MainFrame.Navigate(new Pages.AdministracionPage(_usuario));
                    break;
                case "Estadisticas":
                    MainFrame.Navigate(new Pages.EstadisticasPage(_usuario));
                    break;
            }
        }

        // ── Botones del menú ───────────────────
        private void BtnDashboard_Click(object sender, RoutedEventArgs e) =>
            NavegaryA("Dashboard");

        private void BtnVentas_Click(object sender, RoutedEventArgs e) =>
            NavegaryA("Ventas");

        private void BtnTurnos_Click(object sender, RoutedEventArgs e) =>
            NavegaryA("Turnos");

        private void BtnReportes_Click(object sender, RoutedEventArgs e) =>
            NavegaryA("Reportes");

        private void BtnCaseros_Click(object sender, RoutedEventArgs e) =>
            NavegaryA("Caseros");

        private void BtnInventario_Click(object sender, RoutedEventArgs e) =>
            NavegaryA("Inventario");

        private void BtnAdministracion_Click(object sender, RoutedEventArgs e) =>
            NavegaryA("Administracion");

        private void BtnEstadisticas_Click(object sender, RoutedEventArgs e) =>
            NavegaryA("Estadisticas");

        // ── Cerrar sesión ──────────────────────
        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show(
                "¿Estás seguro que deseas cerrar sesión?",
                "Cerrar Sesión",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                _reloj.Stop();
                var login = new LoginView();
                login.Show();
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _reloj.Stop();
            base.OnClosed(e);
        }
    }
}