// =============================================
// ELITE LAN CENTER - CASEROS PAGE
// =============================================

using EliteLanCenter.Controllers;
using EliteLanCenter.Models;
using System.Windows;
using System.Windows.Controls;

namespace EliteLanCenter.Pages
{
    public partial class CaserosPage : Page
    {
        private readonly Usuario _usuario;
        private Casero? _caseroSeleccionado;

        public CaserosPage(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            CargarCaseros();

            // Enfocar lector RFID
            this.Loaded += (s, e) => TxtRFID.Focus();
        }

        // ── Cargar caseros ─────────────────────
        private void CargarCaseros(string busqueda = "")
        {
            var caseros = string.IsNullOrWhiteSpace(busqueda)
                ? CaseroController.ObtenerTodos()
                : CaseroController.BuscarPorNombre(busqueda);

            ListaCaseros.ItemsSource = caseros;
        }

        // ── Búsqueda ───────────────────────────
        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            CargarCaseros(TxtBuscar.Text.Trim());
        }

        // ── Lector RFID ────────────────────────
        private void TxtRFID_TextChanged(object sender, TextChangedEventArgs e)
        {
            var codigo = TxtRFID.Text.Trim();
            if (codigo.Length < 4) return;

            // Buscar casero por RFID
            var casero = CaseroController.BuscarPorRFID(codigo);

            if (casero != null)
            {
                _caseroSeleccionado = casero;
                PanelClienteRFID.Visibility = Visibility.Visible;
                LblNombreRFID.Text = $"👤 {casero.NombreCompleto}";
                LblSaldoRFID.Text = $"💰 Saldo: {casero.SaldoFormateado}  |  {casero.EstadoSaldo}";

                // Cargar historial
                var historial = CaseroController.ObtenerHistorial(casero.Id);
                ListaHistorial.ItemsSource = historial;

                LblMensaje.Text = "";
            }
            else
            {
                _caseroSeleccionado = null;
                PanelClienteRFID.Visibility = Visibility.Collapsed;
                ListaHistorial.ItemsSource = null;
            }
        }

        // ── Nuevo casero ───────────────────────
        private void BtnNuevoCasero_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new Views.NuevoCaseroView(_usuario);
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();
            CargarCaseros();
        }

        // ── Recargar saldo (desde lista) ───────
        private void BtnRecargar_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var caseroId = (int)btn.Tag;
            MostrarDialogoRecarga(caseroId);
        }

        // ── Consumo (desde lista) ──────────────
        private void BtnConsumo_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var caseroId = (int)btn.Tag;
            MostrarDialogoConsumo(caseroId);
        }

        // ── Recargar rápido (desde RFID) ───────
        private void BtnRecargarRapido_Click(object sender, RoutedEventArgs e)
        {
            if (_caseroSeleccionado == null)
            {
                LblMensaje.Text = "⚠ Acerca una tarjeta al lector primero.";
                return;
            }

            if (!double.TryParse(TxtMonto.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double monto) || monto <= 0)
            {
                LblMensaje.Text = "⚠ Ingresa un monto válido.";
                return;
            }

            var (ok, mensaje) = CaseroController.RecargarSaldo(
                _caseroSeleccionado.Id, monto, _usuario.Id);

            LblMensaje.Text = mensaje;
            LblMensaje.Foreground = ok
                ? new System.Windows.Media.SolidColorBrush(
                  System.Windows.Media.Color.FromRgb(0x01, 0xEF, 0xAC))
                : new System.Windows.Media.SolidColorBrush(
                  System.Windows.Media.Color.FromRgb(0xEF, 0x44, 0x44));

            if (ok)
            {
                TxtMonto.Text = "0.00";
                // Actualizar casero seleccionado
                _caseroSeleccionado = CaseroController.ObtenerPorId(
                    _caseroSeleccionado.Id);
                if (_caseroSeleccionado != null)
                {
                    LblSaldoRFID.Text = $"💰 Saldo: {_caseroSeleccionado.SaldoFormateado}  |  {_caseroSeleccionado.EstadoSaldo}";
                    ListaHistorial.ItemsSource = CaseroController.ObtenerHistorial(
                        _caseroSeleccionado.Id);
                }
                CargarCaseros();
            }
        }

        // ── Consumo rápido (desde RFID) ────────
        private void BtnConsumoRapido_Click(object sender, RoutedEventArgs e)
        {
            if (_caseroSeleccionado == null)
            {
                LblMensaje.Text = "⚠ Acerca una tarjeta al lector primero.";
                return;
            }

            if (!double.TryParse(TxtMonto.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double monto) || monto <= 0)
            {
                LblMensaje.Text = "⚠ Ingresa un monto válido.";
                return;
            }

            var (ok, mensaje) = CaseroController.RegistrarConsumo(
                _caseroSeleccionado.Id, monto, _usuario.Id);

            LblMensaje.Text = mensaje;
            LblMensaje.Foreground = ok
                ? new System.Windows.Media.SolidColorBrush(
                  System.Windows.Media.Color.FromRgb(0x01, 0xEF, 0xAC))
                : new System.Windows.Media.SolidColorBrush(
                  System.Windows.Media.Color.FromRgb(0xEF, 0x44, 0x44));

            if (ok)
            {
                TxtMonto.Text = "0.00";
                _caseroSeleccionado = CaseroController.ObtenerPorId(
                    _caseroSeleccionado.Id);
                if (_caseroSeleccionado != null)
                {
                    LblSaldoRFID.Text = $"💰 Saldo: {_caseroSeleccionado.SaldoFormateado}  |  {_caseroSeleccionado.EstadoSaldo}";
                    ListaHistorial.ItemsSource = CaseroController.ObtenerHistorial(
                        _caseroSeleccionado.Id);
                }
                CargarCaseros();
            }
        }

        // ── Diálogos ───────────────────────────
        private void MostrarDialogoRecarga(int caseroId)
        {
            var casero = CaseroController.ObtenerPorId(caseroId);
            if (casero == null) return;

            var dialog = new Views.InputDialog(
                "Recargar Saldo",
                $"Ingresa el monto a recargar para {casero.Nombre}:",
                "0.00");

            if (dialog.ShowDialog() != true) return;
            var monto = dialog.InputValue;

            if (string.IsNullOrWhiteSpace(monto)) return;

            if (!double.TryParse(monto.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double montoDouble) || montoDouble <= 0)
            {
                MessageBox.Show("Ingresa un monto válido.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (ok, mensaje) = CaseroController.RecargarSaldo(
                caseroId, montoDouble, _usuario.Id);

            MessageBox.Show(mensaje, ok ? "✅ Éxito" : "Error",
                MessageBoxButton.OK,
                ok ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (ok) CargarCaseros();
        }

        private void MostrarDialogoConsumo(int caseroId)
        {
            var casero = CaseroController.ObtenerPorId(caseroId);
            if (casero == null) return;

            var dialog = new Views.InputDialog(
                "Registrar Consumo",
                $"Ingresa el monto consumido por {casero.Nombre}:\nSaldo actual: {casero.SaldoFormateado}",
                "0.00");

            if (dialog.ShowDialog() != true) return;
            var monto = dialog.InputValue;

            if (string.IsNullOrWhiteSpace(monto)) return;

            if (!double.TryParse(monto.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double montoDouble) || montoDouble <= 0)
            {
                MessageBox.Show("Ingresa un monto válido.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (ok, mensaje) = CaseroController.RegistrarConsumo(
                caseroId, montoDouble, _usuario.Id);

            MessageBox.Show(mensaje, ok ? "✅ Éxito" : "Error",
                MessageBoxButton.OK,
                ok ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (ok) CargarCaseros();
        }
    }
}