using ClosedXML.Excel;
using EliteLanCenter.Controllers;
using EliteLanCenter.Models;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace EliteLanCenter.Pages
{
    public partial class EstadisticasPage : Page
    {
        public EstadisticasPage(Usuario? usuario = null)
        {
            InitializeComponent();

            DateDesde.SelectedDate = DateTime.Today.AddDays(-7);
            DateHasta.SelectedDate = DateTime.Today;
        }

        private void BtnPreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                var tag = btn.Tag.ToString();
                DateDesde.SelectedDate = tag switch
                {
                    "7" => DateTime.Today.AddDays(-7),
                    "30" => DateTime.Today.AddDays(-30),
                    "mes" => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                    _ => DateTime.Today.AddDays(-7)
                };
                DateHasta.SelectedDate = DateTime.Today;
            }
        }

        private async void BtnGenerarExcel_Click(object sender, RoutedEventArgs e)
        {
            var desde = DateDesde.SelectedDate;
            var hasta = DateHasta.SelectedDate;

            if (desde == null || hasta == null)
            {
                MessageBox.Show("Selecciona las fechas de inicio y fin.",
                    "Fechas requeridas", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (desde > hasta)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor a la fecha de fin.",
                    "Rango inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Title = "Guardar Reporte Estadístico",
                Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                FileName = $"Reporte_Estadistico_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            BtnGenerarExcel.IsEnabled = false;
            BtnGenerarExcel.Content = "⏳ Generando...";
            LblInfo.Text = "Generando reporte...";

            try
            {
                var filePath = saveDialog.FileName;
                await Task.Run(() => GenerarExcel(filePath, desde.Value, hasta.Value));

                LblInfo.Text = $"✅ Reporte guardado en:\n{filePath}";

                var abrir = MessageBox.Show(
                    "Reporte generado correctamente. ¿Deseas abrir el archivo?",
                    "Reporte Listo",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (abrir == MessageBoxResult.Yes)
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                LblInfo.Text = $"❌ Error: {ex.Message}";
                MessageBox.Show($"Error al generar el reporte:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnGenerarExcel.IsEnabled = true;
                BtnGenerarExcel.Content = "📥 Generar Excel";
            }
        }

        private void GenerarExcel(string filePath, DateTime desde, DateTime hasta)
        {
            var fechaInicio = desde.ToString("yyyy-MM-dd");
            var fechaFin = hasta.ToString("yyyy-MM-dd");

            var resumen = ReporteController.ObtenerResumenGlobal(fechaInicio, fechaFin);
            var diarias = ReporteController.ObtenerVentasDiariasDetalle(fechaInicio, fechaFin);
            var productos = ReporteController.ObtenerProductosMasVendidos(fechaInicio, fechaFin, 50);
            var turnos = ReporteController.ObtenerEstadisticasPorTurno(fechaInicio, fechaFin);

            using var workbook = new XLWorkbook();

            CrearHojaResumen(workbook, resumen, desde, hasta);
            CrearHojaVentasDiarias(workbook, diarias);
            CrearHojaProductos(workbook, productos);
            CrearHojaTurnos(workbook, turnos);
            CrearHojaTopDias(workbook, diarias);
            CrearHojatendencia(workbook, diarias);
            CrearHojaDiaSemana(workbook, diarias);

            workbook.SaveAs(filePath);

            var totalProdVendidos = diarias.Sum(d => d.totalProductosVendidos);
            Dispatcher.Invoke(() => MostrarResumen(resumen, totalProdVendidos));
        }

        private static void CrearHojaResumen(XLWorkbook wb,
            (double totalMaquinas, double totalProductos, double totalYape, double totalDescuentos, double totalLiquido, double totalEfectivo, int totalTransacciones, int totalDias) resumen,
            DateTime desde, DateTime hasta)
        {
            var ws = wb.Worksheets.Add("Resumen General");
            var c = new Celda(ws);

            c.Escribir(1, 1, "REPORTE ESTADÍSTICO - ELITE LAN CENTER", true, 16);
            c.Escribir(2, 1, $"Período: {desde:dd/MM/yyyy} - {hasta:dd/MM/yyyy}", false, 12);

            ws.Cell(4, 1).InsertTable(new[]
            {
                new { Indicador = "💰 Ingreso Máquinas",     Valor = $"S/ {resumen.totalMaquinas:F2}" },
                new { Indicador = "🛒 Venta Productos",        Valor = $"S/ {resumen.totalProductos:F2}" },
                new { Indicador = "💳 Yape / Transferencia",   Valor = $"S/ {resumen.totalYape:F2}" },
                new { Indicador = "🧾 Efectivo",               Valor = $"S/ {resumen.totalEfectivo:F2}" },
                new { Indicador = "📉 Descuentos",             Valor = $"S/ {resumen.totalDescuentos:F2}" },
                new { Indicador = "🏆 Total Líquido",           Valor = $"S/ {resumen.totalLiquido:F2}" },
                new { Indicador = "📊 Transacciones",          Valor = resumen.totalTransacciones.ToString() },
                new { Indicador = "📅 Días con reportes",     Valor = resumen.totalDias.ToString() },
            }, "ELITE");

            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 35;
            ws.Column(2).Width = 25;
        }

        private static void CrearHojaVentasDiarias(XLWorkbook wb,
            List<(string fecha, double maquinas, double productos, double yape, double descuentos, double liquido, double efectivo, int transacciones, int totalProductosVendidos)> diarias)
        {
            var ws = wb.Worksheets.Add("Ventas Diarias");
            var c = new Celda(ws);

            c.Escribir(1, 1, "VENTAS DIARIAS", true, 14);

            var datos = diarias.Select(d => new
            {
                Fecha = d.fecha,
                Máquinas = $"S/ {d.maquinas:F2}",
                Productos = $"S/ {d.productos:F2}",
                Total_Día = $"S/ {d.maquinas + d.productos:F2}",
                Yape = $"S/ {d.yape:F2}",
                Efectivo = $"S/ {d.efectivo:F2}",
                Descuentos = $"S/ {d.descuentos:F2}",
                Líquido = $"S/ {d.liquido:F2}",
                Transacciones = d.transacciones,
                Prod_Vendidos = d.totalProductosVendidos
            }).ToList();

            if (datos.Count > 0)
                ws.Cell(3, 1).InsertTable(datos, "Días");

            ws.Columns().AdjustToContents();
        }

        private static void CrearHojaProductos(XLWorkbook wb,
            List<(string nombre, int cantidad, double total)> productos)
        {
            var ws = wb.Worksheets.Add("Productos Vendidos");
            var c = new Celda(ws);

            c.Escribir(1, 1, "PRODUCTOS MÁS VENDIDOS", true, 14);

            var datos = productos.Select((p, i) => new
            {
                Posición = i + 1,
                Producto = p.nombre,
                Cantidad = p.cantidad,
                Total = $"S/ {p.total:F2}"
            }).ToList();

            if (datos.Count > 0)
                ws.Cell(3, 1).InsertTable(datos, "Productos");

            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 10;
            ws.Column(2).Width = 35;
        }

        private static void CrearHojaTurnos(XLWorkbook wb,
            (double manna, double tarde, double noche) turnos)
        {
            var ws = wb.Worksheets.Add("Turnos");
            var c = new Celda(ws);

            c.Escribir(1, 1, "INGRESOS POR TURNO", true, 14);

            var total = turnos.manna + turnos.tarde + turnos.noche;

            var datos = new[]
            {
                new { Turno = "🌅 Mañana",  Total = $"S/ {turnos.manna:F2}", Porcentaje = total > 0 ? $"{turnos.manna / total * 100:F1}%" : "0%" },
                new { Turno = "☀️ Tarde",   Total = $"S/ {turnos.tarde:F2}", Porcentaje = total > 0 ? $"{turnos.tarde / total * 100:F1}%" : "0%" },
                new { Turno = "🌙 Noche",   Total = $"S/ {turnos.noche:F2}", Porcentaje = total > 0 ? $"{turnos.noche / total * 100:F1}%" : "0%" },
            };

            ws.Cell(3, 1).InsertTable(datos, "Turnos");
            ws.Columns().AdjustToContents();
        }

        private static void CrearHojaTopDias(XLWorkbook wb,
            List<(string fecha, double maquinas, double productos, double yape, double descuentos, double liquido, double efectivo, int transacciones, int totalProductosVendidos)> diarias)
        {
            var ws = wb.Worksheets.Add("Top Días");
            var c = new Celda(ws);

            c.Escribir(1, 1, "DÍAS CON MÁS INGRESOS (Top 10)", true, 14);

            var top = diarias
                .OrderByDescending(d => d.maquinas + d.productos)
                .Take(10)
                .Select((d, i) => new
                {
                    Posición = i + 1,
                    Fecha = d.fecha,
                    Total = $"S/ {(d.maquinas + d.productos):F2}",
                    Máquinas = $"S/ {d.maquinas:F2}",
                    Productos = $"S/ {d.productos:F2}",
                    Transacciones = d.transacciones
                }).ToList();

            if (top.Count > 0)
                ws.Cell(3, 1).InsertTable(top, "Top");

            ws.Columns().AdjustToContents();
        }

        private static void CrearHojatendencia(XLWorkbook wb,
            List<(string fecha, double maquinas, double productos, double yape, double descuentos, double liquido, double efectivo, int transacciones, int totalProductosVendidos)> diarias)
        {
            var ws = wb.Worksheets.Add("Tendencia");
            var c = new Celda(ws);

            c.Escribir(1, 1, "TENDENCIA DE VENTAS (Día vs Día anterior)", true, 14);

            var lista = new List<dynamic>();

            for (int i = 0; i < diarias.Count; i++)
            {
                var d = diarias[i];
                var totalHoy = d.maquinas + d.productos;
                double totalAyer = 0;
                if (i > 0)
                    totalAyer = diarias[i - 1].maquinas + diarias[i - 1].productos;

                var diff = totalHoy - totalAyer;
                var pct = totalAyer > 0 ? diff / totalAyer * 100 : 0;

                lista.Add(new
                {
                    Fecha = d.fecha,
                    Total_Hoy = $"S/ {totalHoy:F2}",
                    Total_Día_Anterior = i > 0 ? $"S/ {totalAyer:F2}" : "—",
                    Diferencia = $"S/ {diff:F2}",
                    Cambio = i > 0 ? $"{pct:F1}%" : "—"
                });
            }

            if (lista.Count > 0)
                ws.Cell(3, 1).InsertTable(lista.ToArray(), "Tendencia");

            ws.Columns().AdjustToContents();
        }

        private static void CrearHojaDiaSemana(XLWorkbook wb,
            List<(string fecha, double maquinas, double productos, double yape, double descuentos, double liquido, double efectivo, int transacciones, int totalProductosVendidos)> diarias)
        {
            var ws = wb.Worksheets.Add("Día Semana");
            var c = new Celda(ws);

            c.Escribir(1, 1, "VENTAS POR DÍA DE LA SEMANA", true, 14);

            var cult = new System.Globalization.CultureInfo("es-PE");
            var agrupado = diarias
                .GroupBy(d =>
                {
                    if (DateTime.TryParse(d.fecha, out var dt))
                        return cult.DateTimeFormat.GetDayName(dt.DayOfWeek);
                    return d.fecha;
                })
                .Select(g => new
                {
                    Día = g.Key,
                    Promedio = $"S/ {g.Average(d => d.maquinas + d.productos):F2}",
                    Total = $"S/ {g.Sum(d => d.maquinas + d.productos):F2}",
                    Días = g.Count(),
                    Máquinas_Prom = $"S/ {g.Average(d => d.maquinas):F2}",
                    Productos_Prom = $"S/ {g.Average(d => d.productos):F2}"
                })
                .ToList();

            if (agrupado.Count > 0)
                ws.Cell(3, 1).InsertTable(agrupado, "Semana");

            ws.Columns().AdjustToContents();
        }

        private void MostrarResumen(
            (double totalMaquinas, double totalProductos, double totalYape, double totalDescuentos, double totalLiquido, double totalEfectivo, int totalTransacciones, int totalDias) resumen,
            int totalProdVendidos)
        {
            PanelResumen.Visibility = Visibility.Visible;
            LblResumenMaquinas.Text = $"S/ {resumen.totalMaquinas:F2}";
            LblResumenProductos.Text = $"S/ {resumen.totalProductos:F2}";
            LblResumenYape.Text = $"S/ {resumen.totalYape:F2}";
            LblResumenEfectivo.Text = $"S/ {resumen.totalEfectivo:F2}";
            LblResumenTransacciones.Text = resumen.totalTransacciones.ToString();
            LblResumenProdVendidos.Text = totalProdVendidos.ToString();
            LblResumenDescuentos.Text = $"S/ {resumen.totalDescuentos:F2}";
            LblResumenLiquido.Text = $"S/ {resumen.totalLiquido:F2}";
        }
    }

    internal class Celda
    {
        private readonly IXLWorksheet _ws;

        public Celda(IXLWorksheet ws) => _ws = ws;

        public void Escribir(int fila, int col, string texto, bool negrita = false, int tamaño = 11)
        {
            var cell = _ws.Cell(fila, col);
            cell.Value = texto;
            if (negrita) cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = tamaño;
        }
    }
}
