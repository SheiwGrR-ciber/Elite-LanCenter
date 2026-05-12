// =============================================
// ELITE LAN CENTER - PDF GENERATOR
// =============================================

using EliteLanCenter.Models;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EliteLanCenter.Reports
{
    public static class PdfGenerator
    {
        static PdfGenerator()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static string GenerarReporteTurno(
            Reporte reporte,
            List<(string producto, int cantidad, double total)> productos,
            List<Producto> todosProductos)
        {
            var folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EliteLanCenter",
                "Reportes");

            Directory.CreateDirectory(folderPath);

            var fileName = $"Reporte_{reporte.FechaFormateada.Replace("/", "-")}_{DateTime.Now:HHmmss}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeHeader(c, reporte));
                    page.Content().Element(c => ComposeContent(c, reporte, productos));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf(filePath);

            return filePath;
        }

        private static void ComposeHeader(IContainer container, Reporte reporte)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("ELITE LAN CENTER")
                    .FontSize(20).Bold().FontColor("#01EFAC");

                column.Item().AlignCenter().Text("Reporte de Cierre de Turno")
                    .FontSize(14).SemiBold();

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Operador: {reporte.NombreOperador}").FontSize(10);
                        col.Item().Text($"Fecha: {reporte.FechaFormateada}").FontSize(10);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text($"Turno: {reporte.TipoTurno}").FontSize(10);
                        col.Item().Text($"Hora Apertura: {reporte.HoraApertura}").FontSize(10);
                        if (!string.IsNullOrEmpty(reporte.HoraCierre) && reporte.HoraCierre != "—")
                            col.Item().Text($"Hora Cierre: {reporte.HoraCierre}").FontSize(10);
                    });
                });

                column.Item().PaddingTop(10).LineHorizontal(1).LineColor("#CCCCCC");
            });
        }

        private static void ComposeContent(IContainer container, Reporte reporte,
            List<(string producto, int cantidad, double total)> productos)
        {
            container.PaddingTop(20).Column(column =>
            {
                column.Item().Text("RESUMEN DE INGRESOS")
                    .FontSize(12).Bold().FontColor("#01EFAC");

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1);
                    });

                    table.Cell().Element(CellStyle).Text("Ingreso Máquinas (PanCafe)");
                    table.Cell().Element(CellStyle).AlignRight().Text(reporte.IngresoMaquinasFormateado);

                    table.Cell().Element(CellStyle).Text("Total Productos");
                    table.Cell().Element(CellStyle).AlignRight().Text(reporte.TotalProductosFormateado);

                    table.Cell().Element(CellStyle).Background("#F0F0F0").Text("Total Bruto").Bold();
                    table.Cell().Element(CellStyle).Background("#F0F0F0").AlignRight().Text(reporte.TotalBrutoFormateado).Bold();
                });

                column.Item().PaddingTop(15).Text("DEDUCCIONES")
                    .FontSize(12).Bold().FontColor("#01EFAC");

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1);
                    });

                    table.Cell().Element(CellStyle).Text("Descuentos");
                    table.Cell().Element(CellStyle).AlignRight().Text(reporte.DescuentosFormateado);

                    table.Cell().Element(CellStyle).Text("Monto Yape/Transferencia");
                    table.Cell().Element(CellStyle).AlignRight().Text(reporte.MontoYapeFormateado);

                    table.Cell().Element(CellStyle).Background("#F0F0F0").Text("Total Líquido").Bold();
                    table.Cell().Element(CellStyle).Background("#F0F0F0").AlignRight().Text(reporte.TotalLiquidoFormateado).Bold();

                    table.Cell().Element(CellStyle).Background("#01EFAC").Text("EFECTIVO A ENTREGAR").Bold()
                        .FontColor("White");
                    table.Cell().Element(CellStyle).Background("#01EFAC").AlignRight().Text(reporte.EfectivoFormateado).Bold()
                        .FontColor("White");
                });

                if (productos.Any())
                {
                    column.Item().PaddingTop(20).Text("DETALLE DE VENTAS")
                        .FontSize(12).Bold().FontColor("#01EFAC");

                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("Producto");
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Cantidad");
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Total");
                        });

                        foreach (var item in productos)
                        {
                            table.Cell().Element(CellStyle).Text(item.producto);
                            table.Cell().Element(CellStyle).AlignRight().Text(item.cantidad.ToString());
                            table.Cell().Element(CellStyle).AlignRight().Text($"S/ {item.total:F2}");
                        }
                    });
                }

                if (!string.IsNullOrEmpty(reporte.ImagenPanCafe1) ||
                    !string.IsNullOrEmpty(reporte.ImagenPanCafe2) ||
                    !string.IsNullOrEmpty(reporte.ImagenYape))
                {
                    column.Item().PaddingTop(20).Text("EVIDENCIA FOTOGRÁFICA")
                        .FontSize(12).Bold().FontColor("#01EFAC");

                    column.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().AlignCenter().Column(col =>
                        {
                            if (!string.IsNullOrEmpty(reporte.ImagenPanCafe1))
                            {
                                col.Item().Text("PanCafe 1").FontSize(9).FontColor("#888");
                                col.Item().Image(reporte.ImagenPanCafe1);
                            }
                        });

                        row.RelativeItem().AlignCenter().Column(col =>
                        {
                            if (!string.IsNullOrEmpty(reporte.ImagenPanCafe2))
                            {
                                col.Item().Text("PanCafe 2").FontSize(9).FontColor("#888");
                                col.Item().Image(reporte.ImagenPanCafe2);
                            }
                        });

                        row.RelativeItem().AlignCenter().Column(col =>
                        {
                            if (!string.IsNullOrEmpty(reporte.ImagenYape))
                            {
                                col.Item().Text("Yape").FontSize(9).FontColor("#888");
                                col.Item().Image(reporte.ImagenYape);
                            }
                        });
                    });
                }
            });
        }

        private static void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(8).FontColor("#888888");
                text.Span(" | ").FontSize(8).FontColor("#888888");
                text.Span("Elite Lan Center").FontSize(8).FontColor("#888888");
            });
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.PaddingVertical(4).PaddingHorizontal(2);
        }

        private static IContainer HeaderCellStyle(IContainer container)
        {
            return container.PaddingVertical(4).PaddingHorizontal(2)
                .Background("#2D3250").DefaultTextStyle(x => x.FontColor("#FFFFFF").Bold());
        }
    }
}
