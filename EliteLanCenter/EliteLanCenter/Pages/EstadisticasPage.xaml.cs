// =============================================
// ELITE LAN CENTER - ESTADISTICAS PAGE
// =============================================

using EliteLanCenter.Controllers;
using EliteLanCenter.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Windows;
using System.Windows.Controls;

namespace EliteLanCenter.Pages
{
    public partial class EstadisticasPage : Page
    {
        private int _diasSeleccionados = 7;

        public EstadisticasPage(Usuario? usuario = null)
        {
            InitializeComponent();
            CargarEstadisticas();
        }

        private void BtnFiltro_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                _diasSeleccionados = btn.Tag.ToString() switch
                {
                    "7" => 7,
                    "30" => 30,
                    "mes" => DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month),
                    _ => 7
                };

                Btn7Dias.Style = _diasSeleccionados == 7 ? (Style)FindResource("AccentButton") : (Style)FindResource("PrimaryButton");
                Btn30Dias.Style = _diasSeleccionados == 30 ? (Style)FindResource("AccentButton") : (Style)FindResource("PrimaryButton");
                BtnEsteMes.Style = _diasSeleccionados == DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) ? (Style)FindResource("AccentButton") : (Style)FindResource("PrimaryButton");

                CargarEstadisticas();
            }
        }

        private void CargarEstadisticas()
        {
            LblPeriodo.Text = _diasSeleccionados switch
            {
                7 => "Datos de los últimos 7 días",
                30 => "Datos de los últimos 30 días",
                _ => $"Datos del mes ({_diasSeleccionados} días)"
            };

            var fechaInicio = DateTime.Now.AddDays(-_diasSeleccionados).ToString("yyyy-MM-dd");
            var fechaFin = DateTime.Now.ToString("yyyy-MM-dd");

            CargarResumenGeneral(fechaInicio, fechaFin);
            CargarGraficoVentasDiarias(fechaInicio, fechaFin);
            CargarGraficoTurnos(fechaInicio, fechaFin);
            CargarGraficoProductos(fechaInicio, fechaFin);
            CargarDetalleDias(fechaInicio, fechaFin);
        }

        private void CargarResumenGeneral(string fechaInicio, string fechaFin)
        {
            var reportes = ReporteController.ObtenerEstadisticasSemana(fechaInicio, fechaFin);

            double totalVentas = reportes.Sum(r => r.maquinas + r.productos);
            double totalYape = reportes.Sum(r => r.yape);
            double totalMaquinas = reportes.Sum(r => r.maquinas);
            double totalDescuentos = reportes.Sum(r => r.descuentos);
            double totalLiquido = reportes.Sum(r => r.liquido);
            double totalEfectivo = reportes.Sum(r => r.efectivo);
            int totalTransacciones = reportes.Sum(r => r.transacciones);

            LblVentasTotales.Text = $"S/ {totalVentas:F2}";
            LblTransacciones.Text = totalTransacciones.ToString();
            LblYapeTotal.Text = $"S/ {totalYape:F2}";
            LblMaquinasTotal.Text = $"S/ {totalMaquinas:F2}";
            LblEfectivoTotal.Text = $"S/ {totalEfectivo:F2}";
            LblDescuentosTotal.Text = $"S/ {totalDescuentos:F2}";
            LblLiquidoTotal.Text = $"S/ {totalLiquido:F2}";
        }

        private void CargarGraficoVentasDiarias(string fechaInicio, string fechaFin)
        {
            var reportes = ReporteController.ObtenerEstadisticasSemana(fechaInicio, fechaFin);

            var valores = reportes.Select(r => r.maquinas + r.productos).ToArray();
            var etiquetas = reportes.Select(r => FormatearFecha(r.fecha)).ToArray();

            GraficoVentasDiarias.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = valores,
                    Fill = new SolidColorPaint(SKColor.Parse("#01EFAC")),
                    Stroke = null,
                    MaxBarWidth = 30
                }
            };

            GraficoVentasDiarias.XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = etiquetas,
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#8892B0")),
                    TextSize = 10
                }
            };

            GraficoVentasDiarias.YAxes = new Axis[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#8892B0")),
                    TextSize = 10,
                    Labeler = value => $"S/ {value:F0}"
                }
            };
        }

        private void CargarGraficoTurnos(string fechaInicio, string fechaFin)
        {
            var reportes = ReporteController.ObtenerEstadisticasPorTurno(fechaInicio, fechaFin);

            var colores = new[]
            {
                SKColor.Parse("#524094"),
                SKColor.Parse("#2082A6"),
                SKColor.Parse("#F59E0B")
            };

            GraficoTurnos.Series = new ISeries[]
            {
                new PieSeries<double>
                {
                    Values = new double[] { reportes.manna },
                    Name = "Mañana",
                    Fill = new SolidColorPaint(colores[0])
                },
                new PieSeries<double>
                {
                    Values = new double[] { reportes.tarde },
                    Name = "Tarde",
                    Fill = new SolidColorPaint(colores[1])
                },
                new PieSeries<double>
                {
                    Values = new double[] { reportes.noche },
                    Name = "Noche",
                    Fill = new SolidColorPaint(colores[2])
                }
            };
        }

        private void CargarGraficoProductos(string fechaInicio, string fechaFin)
        {
            var productos = ReporteController.ObtenerProductosMasVendidos(fechaInicio, fechaFin, 5);

            if (productos.Count == 0)
            {
                GraficoProductos.Series = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Values = new double[] { 0 },
                        Fill = new SolidColorPaint(SKColor.Parse("#2D3250"))
                    }
                };
                return;
            }

            var valores = productos.Select(p => (double)p.total).ToArray();
            var etiquetas = productos.Select(p => p.nombre.Length > 10 ? p.nombre.Substring(0, 10) + "..." : p.nombre).ToArray();

            GraficoProductos.Series = new ISeries[]
            {
                new RowSeries<double>
                {
                    Values = valores,
                    Fill = new SolidColorPaint(SKColor.Parse("#524094")),
                    Stroke = null,
                    MaxBarWidth = 30
                }
            };

            GraficoProductos.XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = valores.Select(v => $"S/ {v:F0}").ToArray(),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#8892B0")),
                    TextSize = 10
                }
            };

            GraficoProductos.YAxes = new Axis[]
            {
                new Axis
                {
                    Labels = etiquetas,
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#8892B0")),
                    TextSize = 10
                }
            };
        }

        private void CargarDetalleDias(string fechaInicio, string fechaFin)
        {
            var reportes = ReporteController.ObtenerEstadisticasSemana(fechaInicio, fechaFin);

            var detalle = reportes.Select(r => new
            {
                Fecha = FormatearFecha(r.fecha),
                Ventas = $"S/ {(r.maquinas + r.productos):F2}",
                Transacciones = $"{r.transacciones} ventas"
            }).ToList();

            ListaDias.ItemsSource = detalle;
        }

        private string FormatearFecha(string fecha)
        {
            if (DateTime.TryParse(fecha, out var fechaDate))
            {
                return fechaDate.ToString("dd MMM");
            }
            return fecha;
        }
    }
}
