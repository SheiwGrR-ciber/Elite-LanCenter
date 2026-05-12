// =============================================
// ELITE LAN CENTER - REPORTES PAGE
// =============================================

using EliteLanCenter.Controllers;
using EliteLanCenter.Models;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace EliteLanCenter.Pages
{
    public partial class ReportesPage : Page
    {
        private readonly Usuario _usuario;
        private Turno? _turnoActivo;
        private string? _imagenPanCafe1;
        private string? _imagenPanCafe2;
        private string? _imagenYape;

        public ReportesPage(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            CargarDatos();

            // Habilitar Ctrl+V para pegar imágenes
            this.Loaded += (s, e) =>
            {
                System.Windows.Input.CommandManager.AddPreviewExecutedHandler(
                    this, OnPreviewExecuted);
            };
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

            LblInfoTurno.Text = $"🔄 {_turnoActivo.TipoDescripcion}  |  👤 {_turnoActivo.NombreOperador}  |  📅 {_turnoActivo.FechaFormateada}";

            // Resumen de ventas
            var resumen = VentaController.ResumenPorProducto(_turnoActivo.Id);
            var listaResumen = resumen.Select(r => new
            {
                Producto = r.producto,
                Cantidad = r.cantidad,
                Total = r.total
            }).ToList();

            ListaResumenVentas.ItemsSource = listaResumen;

            // Total productos
            var totalProductos = VentaController.ObtenerTotalTurno(_turnoActivo.Id);
            LblTotalProductos.Text = $"S/ {totalProductos:F2}";
        }

        // ── Calcular totales ───────────────────
        private void BtnCalcular_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(TxtIngresoMaquinas.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double ingresoMaquinas))
            {
                LblMensaje.Text = "⚠ Ingresa un valor válido para el ingreso de máquinas.";
                return;
            }

            if (!double.TryParse(TxtDescuentos.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double descuentos))
            {
                LblMensaje.Text = "⚠ Ingresa un valor válido para los descuentos.";
                return;
            }

            if (!double.TryParse(TxtMontoYape.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double montoYape))
            {
                LblMensaje.Text = "⚠ Ingresa un valor válido para el monto Yape.";
                return;
            }

            var totalProductos = _turnoActivo != null
                ? VentaController.ObtenerTotalTurno(_turnoActivo.Id)
                : 0;

            var totalBruto = Math.Round(ingresoMaquinas + totalProductos, 2);
            var totalLiquido = Math.Round(totalBruto - descuentos, 2);
            var efectivo = Math.Round(totalLiquido - montoYape, 2);

            LblTotalBruto.Text = $"S/ {totalBruto:F2}";
            LblDescuentos.Text = $"S/ {descuentos:F2}";
            LblTotalLiquido.Text = $"S/ {totalLiquido:F2}";
            LblMontoYape.Text = $"S/ {montoYape:F2}";
            LblEfectivo.Text = $"S/ {efectivo:F2}";

            LblMensaje.Text = "";
        }

        // ── Generar PDF ────────────────────────
        private void BtnGenerarPDF_Click(object sender, RoutedEventArgs e)
        {
            if (_turnoActivo == null)
            {
                MessageBox.Show("No hay turno abierto.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(TxtIngresoMaquinas.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double ingresoMaquinas) ||
                !double.TryParse(TxtDescuentos.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double descuentos) ||
                !double.TryParse(TxtMontoYape.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double montoYape))
            {
                LblMensaje.Text = "⚠ Verifica los valores ingresados.";
                return;
            }

            // Crear reporte en base de datos
            var (ok, mensaje, reporteId) = ReporteController.CrearReporte(
                _turnoActivo.Id, ingresoMaquinas, descuentos, montoYape,
                _imagenPanCafe1, _imagenPanCafe2, _imagenYape);

            if (!ok)
            {
                LblMensaje.Text = $"⚠ {mensaje}";
                return;
            }

            // Generar PDF
            var reporte = ReporteController.ObtenerPorTurno(_turnoActivo.Id);
            if (reporte == null) return;

            try
            {
                var pdfPath = Reports.PdfGenerator.GenerarReporteTurno(reporte,
                    VentaController.ResumenPorProducto(_turnoActivo.Id),
                    ProductoController.ObtenerTodos());

                ReporteController.ActualizarPdfPath(reporteId, pdfPath);

                MessageBox.Show($"✅ PDF generado correctamente.\n\nGuardado en:\n{pdfPath}",
                    "PDF Generado", MessageBoxButton.OK, MessageBoxImage.Information);

                LblMensaje.Text = "";
            }
            catch (Exception ex)
            {
                LblMensaje.Text = $"⚠ Error al generar PDF: {ex.Message}";
            }
        }

        // ── Importar imágenes ──────────────────
        private void BtnImportarPanCafe1_Click(object sender, RoutedEventArgs e)
        {
            var ruta = AbrirDialogoImagen();
            if (ruta != null)
            {
                _imagenPanCafe1 = ruta;
                LblImagenPanCafe1.Text = $"✅ {Path.GetFileName(ruta)}";
                LblImagenPanCafe1.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x01, 0xEF, 0xAC));
            }
        }

        private void BtnImportarPanCafe2_Click(object sender, RoutedEventArgs e)
        {
            var ruta = AbrirDialogoImagen();
            if (ruta != null)
            {
                _imagenPanCafe2 = ruta;
                LblImagenPanCafe2.Text = $"✅ {Path.GetFileName(ruta)}";
                LblImagenPanCafe2.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x01, 0xEF, 0xAC));
            }
        }

        private void BtnImportarYape_Click(object sender, RoutedEventArgs e)
        {
            var ruta = AbrirDialogoImagen();
            if (ruta != null)
            {
                _imagenYape = ruta;
                LblImagenYape.Text = $"✅ {Path.GetFileName(ruta)}";
                LblImagenYape.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x01, 0xEF, 0xAC));
            }
        }

        private string? AbrirDialogoImagen()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Seleccionar imagen",
                Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp"
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        // ── Ctrl+V para pegar imágenes ─────────
        private void OnPreviewExecuted(object sender,
            System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (e.Command != System.Windows.Input.ApplicationCommands.Paste) return;
            if (!System.Windows.Clipboard.ContainsImage()) return;

            var imagen = System.Windows.Clipboard.GetImage();
            if (imagen == null) return;

            // Guardar imagen del clipboard como archivo temporal
            var tempPath = Path.Combine(Path.GetTempPath(),
                $"elite_{DateTime.Now:yyyyMMddHHmmss}.png");

            using var stream = new FileStream(tempPath, FileMode.Create);
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(
                System.Windows.Media.Imaging.BitmapFrame.Create(imagen));
            encoder.Save(stream);

            // Preguntar a qué campo asignar
            var result = MessageBox.Show(
                "¿A qué campo deseas pegar la imagen?\n\n" +
                "Sí = PanCafe (siguiente disponible)\n" +
                "No = Yape",
                "Pegar imagen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (_imagenPanCafe1 == null)
                {
                    _imagenPanCafe1 = tempPath;
                    LblImagenPanCafe1.Text = "✅ Imagen pegada";
                    LblImagenPanCafe1.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(0x01, 0xEF, 0xAC));
                }
                else
                {
                    _imagenPanCafe2 = tempPath;
                    LblImagenPanCafe2.Text = "✅ Imagen pegada";
                    LblImagenPanCafe2.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(0x01, 0xEF, 0xAC));
                }
            }
            else
            {
                _imagenYape = tempPath;
                LblImagenYape.Text = "✅ Imagen pegada";
                LblImagenYape.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x01, 0xEF, 0xAC));
            }

            e.Handled = true;
        }
    }
}