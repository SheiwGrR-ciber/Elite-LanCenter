// =============================================
// ELITE LAN CENTER - VENTAS PAGE
// =============================================

using EliteLanCenter.Controllers;
using EliteLanCenter.Models;
using System.Windows;
using System.Windows.Controls;

namespace EliteLanCenter.Pages
{
    public partial class VentasPage : Page
    {
        private readonly Usuario _usuario;
        private Turno? _turnoActivo;

        public VentasPage(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            CargarDatos();
        }

        // ── Cargar datos ───────────────────────
        private void CargarDatos()
        {
            _turnoActivo = TurnoController.ObtenerTurnoAbierto();

            if (_turnoActivo == null)
            {
                LblInfoTurno.Text = "⚠ No hay turno abierto.";
                return;
            }

            LblInfoTurno.Text = $"🔄 {_turnoActivo.TipoDescripcion}  |  👤 {_turnoActivo.NombreOperador}";

            // Cargar productos
            var productos = ProductoController.ObtenerTodos();
            ListaProductos.ItemsSource = productos;

            // Cargar ventas
            CargarVentas();

            // Cargar fiados
            CargarFiados();
        }

        // ── Cargar ventas del turno ────────────
        private void CargarVentas()
        {
            if (_turnoActivo == null) return;

            PanelVentas.Children.Clear();

            var ventas = VentaController.ObtenerVentasPorTurno(_turnoActivo.Id);

            foreach (var venta in ventas.Where(v => !v.EsFiadoPendiente))
            {
                var fila = new Border
                {
                    Padding = new Thickness(8, 6, 8, 6),
                    Margin = new Thickness(0, 2, 0, 2),
                    CornerRadius = new CornerRadius(6),
                    Background = new System.Windows.Media.SolidColorBrush(
                                       System.Windows.Media.Color.FromRgb(0x2D, 0x32, 0x50))
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var info = new StackPanel();
                info.Children.Add(new TextBlock
                {
                    Text = $"{venta.NombreProducto} x{venta.Cantidad}",
                    FontSize = 12,
                    Foreground = new System.Windows.Media.SolidColorBrush(
                                 System.Windows.Media.Color.FromRgb(0xE8, 0xEA, 0xF6))
                });
                info.Children.Add(new TextBlock
                {
                    Text = venta.HoraVenta,
                    FontSize = 11,
                    Foreground = new System.Windows.Media.SolidColorBrush(
                                 System.Windows.Media.Color.FromRgb(0x88, 0x92, 0xB0))
                });

                var total = new TextBlock
                {
                    Text = venta.TotalFormateado,
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new System.Windows.Media.SolidColorBrush(
                                         System.Windows.Media.Color.FromRgb(0x01, 0xEF, 0xAC)),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                Grid.SetColumn(info, 0);
                Grid.SetColumn(total, 1);
                grid.Children.Add(info);
                grid.Children.Add(total);
                fila.Child = grid;
                PanelVentas.Children.Add(fila);
            }

            // Actualizar total
            var totalTurno = VentaController.ObtenerTotalTurno(_turnoActivo.Id);
            LblTotal.Text = $"S/ {totalTurno:F2}";
        }

        // ── Cargar fiados ──────────────────────
        private void CargarFiados()
        {
            if (_turnoActivo == null) return;
            var fiados = VentaController.ObtenerFiadosPendientes(_turnoActivo.Id);
            ListaFiados.ItemsSource = fiados;
        }

        // ── Registrar venta ────────────────────
        private void BtnVender_Click(object sender, RoutedEventArgs e)
        {
            if (_turnoActivo == null)
            {
                MessageBox.Show("No hay turno abierto.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var btn = (Button)sender;
            var productoId = (int)btn.Tag;

            // Obtener cantidad del TextBox de la misma fila
            var panel = VisualTreeHelper_FindParent<Grid>(btn);
            var txtCantidad = panel?.Children.OfType<TextBox>().FirstOrDefault();
            if (txtCantidad == null) return;

            if (!int.TryParse(txtCantidad.Text.Trim(), out int cantidad) || cantidad <= 0)
            {
                LblMensaje.Text = "⚠ Ingresa una cantidad válida.";
                LblMensaje.Foreground = new System.Windows.Media.SolidColorBrush(
                                        System.Windows.Media.Color.FromRgb(0xEF, 0x44, 0x44));
                return;
            }

            var esFiado = ChkFiado.IsChecked == true;
            var (ok, mensaje, _) = VentaController.RegistrarVenta(
                _turnoActivo.Id, productoId, cantidad, esFiado);

            LblMensaje.Text = mensaje;
            LblMensaje.Foreground = ok
                ? new System.Windows.Media.SolidColorBrush(
                  System.Windows.Media.Color.FromRgb(0x01, 0xEF, 0xAC))
                : new System.Windows.Media.SolidColorBrush(
                  System.Windows.Media.Color.FromRgb(0xEF, 0x44, 0x44));

            if (ok)
            {
                txtCantidad.Text = "1";
                CargarDatos();
            }
        }

        // ── Pagar fiado ────────────────────────
        private void BtnPagarFiado_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var ventaId = (int)btn.Tag;

            var (ok, mensaje) = VentaController.PagarFiado(ventaId);

            LblMensaje.Text = mensaje;
            if (ok) CargarDatos();
        }

        // ── Helper para encontrar parent ───────
        private static T? VisualTreeHelper_FindParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T typedParent) return typedParent;
            return VisualTreeHelper_FindParent<T>(parent);
        }
    }
}