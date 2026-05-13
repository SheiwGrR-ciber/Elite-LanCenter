// =============================================
// ELITE LAN CENTER - CONTROLLER DE REPORTES
// =============================================

using EliteLanCenter.Database;
using EliteLanCenter.Models;

namespace EliteLanCenter.Controllers
{
    public class ReporteController
    {
        // Crear reporte de cierre de turno
        public static (bool ok, string mensaje, int reporteId) CrearReporte(
            int turnoId, double ingresoMaquinas,
            double descuentos, double montoYape,
            string? imagenPanCafe1 = null,
            string? imagenPanCafe2 = null,
            string? imagenYape = null)
        {
            // Obtener turno
            var turno = TurnoController.ObtenerTurnoAbierto();
            if (turno == null)
                return (false, "No hay turno abierto.", 0);

            if (ingresoMaquinas < 0)
                return (false, "El ingreso de máquinas no puede ser negativo.", 0);

            if (descuentos < 0)
                return (false, "Los descuentos no pueden ser negativos.", 0);

            if (montoYape < 0)
                return (false, "El monto Yape no puede ser negativo.", 0);

            // Calcular totales automáticamente
            var totalProductos = VentaController.ObtenerTotalTurno(turnoId);
            var totalBruto = Math.Round(ingresoMaquinas + totalProductos, 2);
            var totalLiquido = Math.Round(totalBruto - descuentos, 2);
            var efectivo = Math.Round(totalLiquido - montoYape, 2);

            if (efectivo < 0)
                return (false, "El monto Yape no puede ser mayor al total líquido.", 0);

            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                command.CommandText = @"
                    INSERT INTO Reportes (
                        TurnoId, IngresoMaquinas, TotalProductos,
                        TotalBruto, Descuentos, TotalLiquido,
                        MontoYape, Efectivo,
                        ImagenPanCafe1, ImagenPanCafe2, ImagenYape
                    ) VALUES (
                        @turnoId, @ingresoMaquinas, @totalProductos,
                        @totalBruto, @descuentos, @totalLiquido,
                        @montoYape, @efectivo,
                        @imagenPanCafe1, @imagenPanCafe2, @imagenYape
                    );
                    SELECT last_insert_rowid();
                ";

                command.Parameters.AddWithValue("@turnoId", turnoId);
                command.Parameters.AddWithValue("@ingresoMaquinas", ingresoMaquinas);
                command.Parameters.AddWithValue("@totalProductos", totalProductos);
                command.Parameters.AddWithValue("@totalBruto", totalBruto);
                command.Parameters.AddWithValue("@descuentos", descuentos);
                command.Parameters.AddWithValue("@totalLiquido", totalLiquido);
                command.Parameters.AddWithValue("@montoYape", montoYape);
                command.Parameters.AddWithValue("@efectivo", efectivo);
                command.Parameters.AddWithValue("@imagenPanCafe1", imagenPanCafe1 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@imagenPanCafe2", imagenPanCafe2 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@imagenYape", imagenYape ?? (object)DBNull.Value);

                var reporteId = (int)(long)command.ExecuteScalar()!;
                return (true, "Reporte creado correctamente.", reporteId);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", 0);
            }
        }

        // Obtener reporte por turno
        public static Reporte? ObtenerPorTurno(int turnoId)
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 
                    r.Id, r.TurnoId, u.Nombre, t.Tipo, t.Fecha,
                    t.AbiertaEn, t.CerradaEn,
                    r.IngresoMaquinas, r.TotalProductos, r.TotalBruto,
                    r.Descuentos, r.TotalLiquido, r.MontoYape, r.Efectivo,
                    r.ImagenPanCafe1, r.ImagenPanCafe2, r.ImagenYape,
                    r.PdfPath, r.CreadoEn
                FROM Reportes r
                JOIN Turnos   t ON t.Id = r.TurnoId
                JOIN Usuarios u ON u.Id = t.UsuarioId
                WHERE r.TurnoId = @turnoId
            ";

            command.Parameters.AddWithValue("@turnoId", turnoId);

            using var reader = command.ExecuteReader();

            if (reader.Read())
                return MapearReporte(reader);

            return null;
        }

        // Obtener reportes recientes
        public static List<Reporte> ObtenerRecientes(int limite = 30)
        {
            var reportes = new List<Reporte>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 
                    r.Id, r.TurnoId, u.Nombre, t.Tipo, t.Fecha,
                    t.AbiertaEn, t.CerradaEn,
                    r.IngresoMaquinas, r.TotalProductos, r.TotalBruto,
                    r.Descuentos, r.TotalLiquido, r.MontoYape, r.Efectivo,
                    r.ImagenPanCafe1, r.ImagenPanCafe2, r.ImagenYape,
                    r.PdfPath, r.CreadoEn
                FROM Reportes r
                JOIN Turnos   t ON t.Id = r.TurnoId
                JOIN Usuarios u ON u.Id = t.UsuarioId
                ORDER BY r.CreadoEn DESC
                LIMIT @limite
            ";

            command.Parameters.AddWithValue("@limite", limite);

            using var reader = command.ExecuteReader();

            while (reader.Read())
                reportes.Add(MapearReporte(reader));

            return reportes;
        }

        // Actualizar ruta del PDF
        public static void ActualizarPdfPath(int reporteId, string pdfPath)
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                UPDATE Reportes SET PdfPath = @pdfPath WHERE Id = @id
            ";

            command.Parameters.AddWithValue("@pdfPath", pdfPath);
            command.Parameters.AddWithValue("@id", reporteId);
            command.ExecuteNonQuery();
        }

        // Obtener estadísticas semanales
        public static List<(string fecha, double maquinas, double productos, double yape, double descuentos, double liquido, double efectivo, int transacciones)>
            ObtenerEstadisticasSemana(string fechaInicio, string fechaFin)
        {
            var estadisticas = new List<(string, double, double, double, double, double, double, int)>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 
                    t.Fecha,
                    COALESCE(SUM(r.IngresoMaquinas), 0) AS Maquinas,
                    COALESCE(SUM(r.TotalProductos), 0) AS Productos,
                    COALESCE(SUM(r.MontoYape), 0) AS Yape,
                    COALESCE(SUM(r.Descuentos), 0) AS Descuentos,
                    COALESCE(SUM(r.TotalLiquido), 0) AS Liquido,
                    COALESCE(SUM(r.Efectivo), 0) AS Efectivo,
                    COUNT(r.Id) AS Transacciones
                FROM Turnos t
                LEFT JOIN Reportes r ON r.TurnoId = t.Id
                WHERE t.Fecha BETWEEN @inicio AND @fin
                GROUP BY t.Fecha
                ORDER BY t.Fecha ASC
            ";

            command.Parameters.AddWithValue("@inicio", fechaInicio);
            command.Parameters.AddWithValue("@fin", fechaFin);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                estadisticas.Add((
                    reader.GetString(0),
                    reader.GetDouble(1),
                    reader.GetDouble(2),
                    reader.GetDouble(3),
                    reader.GetDouble(4),
                    reader.GetDouble(5),
                    reader.GetDouble(6),
                    reader.GetInt32(7)
                ));
            }

            return estadisticas;
        }

        // Obtener estadísticas por turno
        public static (double manna, double tarde, double noche) ObtenerEstadisticasPorTurno(string fechaInicio, string fechaFin)
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 
                    t.Tipo,
                    COALESCE(SUM(r.IngresoMaquinas + r.TotalProductos), 0) AS Total
                FROM Turnos t
                LEFT JOIN Reportes r ON r.TurnoId = t.Id
                WHERE t.Fecha BETWEEN @inicio AND @fin
                GROUP BY t.Tipo
            ";

            command.Parameters.AddWithValue("@inicio", fechaInicio);
            command.Parameters.AddWithValue("@fin", fechaFin);

            double manna = 0, tarde = 0, noche = 0;

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var tipo = reader.GetString(0);
                var total = reader.GetDouble(1);

                switch (tipo)
                {
                    case "Mañana": manna = total; break;
                    case "Tarde": tarde = total; break;
                    case "Noche": noche = total; break;
                }
            }

            return (manna, tarde, noche);
        }

        // Obtener productos más vendidos
        public static List<(string nombre, int cantidad, double total)> ObtenerProductosMasVendidos(
            string fechaInicio, string fechaFin, int limite = 10)
        {
            var productos = new List<(string, int, double)>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 
                    p.Nombre,
                    SUM(v.Cantidad) AS CantidadTotal,
                    SUM(v.Total) AS TotalVentas
                FROM Ventas v
                JOIN Productos p ON p.Id = v.ProductoId
                JOIN Turnos t ON t.Id = v.TurnoId
                WHERE t.Fecha BETWEEN @inicio AND @fin
                GROUP BY p.Id, p.Nombre
                ORDER BY CantidadTotal DESC
                LIMIT @limite
            ";

            command.Parameters.AddWithValue("@inicio", fechaInicio);
            command.Parameters.AddWithValue("@fin", fechaFin);
            command.Parameters.AddWithValue("@limite", limite);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                productos.Add((
                    reader.GetString(0),
                    reader.GetInt32(1),
                    reader.GetDouble(2)
                ));
            }

            return productos;
        }

        // Obtener ventas diarias detalladas (productos + máquinas)
        public static List<(string fecha, double maquinas, double productos, double yape, double descuentos, double liquido, double efectivo, int transacciones, int totalProductosVendidos)>
            ObtenerVentasDiariasDetalle(string fechaInicio, string fechaFin)
        {
            var lista = new List<(string, double, double, double, double, double, double, int, int)>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 
                    t.Fecha,
                    COALESCE(SUM(r.IngresoMaquinas), 0) AS Maquinas,
                    COALESCE(SUM(r.TotalProductos), 0) AS Productos,
                    COALESCE(SUM(r.MontoYape), 0) AS Yape,
                    COALESCE(SUM(r.Descuentos), 0) AS Descuentos,
                    COALESCE(SUM(r.TotalLiquido), 0) AS Liquido,
                    COALESCE(SUM(r.Efectivo), 0) AS Efectivo,
                    COUNT(r.Id) AS Transacciones,
                    COALESCE((SELECT SUM(v.Cantidad) FROM Ventas v JOIN Turnos t2 ON t2.Id = v.TurnoId WHERE t2.Fecha = t.Fecha), 0) AS TotalProdVendidos
                FROM Turnos t
                LEFT JOIN Reportes r ON r.TurnoId = t.Id
                WHERE t.Fecha BETWEEN @inicio AND @fin
                GROUP BY t.Fecha
                ORDER BY t.Fecha ASC
            ";

            command.Parameters.AddWithValue("@inicio", fechaInicio);
            command.Parameters.AddWithValue("@fin", fechaFin);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add((
                    reader.GetString(0),
                    reader.GetDouble(1),
                    reader.GetDouble(2),
                    reader.GetDouble(3),
                    reader.GetDouble(4),
                    reader.GetDouble(5),
                    reader.GetDouble(6),
                    reader.GetInt32(7),
                    reader.GetInt32(8)
                ));
            }

            return lista;
        }

        // Obtener resumen global del período
        public static (double totalMaquinas, double totalProductos, double totalYape, double totalDescuentos, double totalLiquido, double totalEfectivo, int totalTransacciones, int totalDias) 
            ObtenerResumenGlobal(string fechaInicio, string fechaFin)
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 
                    COALESCE(SUM(r.IngresoMaquinas), 0),
                    COALESCE(SUM(r.TotalProductos), 0),
                    COALESCE(SUM(r.MontoYape), 0),
                    COALESCE(SUM(r.Descuentos), 0),
                    COALESCE(SUM(r.TotalLiquido), 0),
                    COALESCE(SUM(r.Efectivo), 0),
                    COUNT(r.Id),
                    COUNT(DISTINCT t.Fecha)
                FROM Turnos t
                LEFT JOIN Reportes r ON r.TurnoId = t.Id
                WHERE t.Fecha BETWEEN @inicio AND @fin
            ";

            command.Parameters.AddWithValue("@inicio", fechaInicio);
            command.Parameters.AddWithValue("@fin", fechaFin);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return (
                    reader.GetDouble(0),
                    reader.GetDouble(1),
                    reader.GetDouble(2),
                    reader.GetDouble(3),
                    reader.GetDouble(4),
                    reader.GetDouble(5),
                    reader.GetInt32(6),
                    reader.GetInt32(7)
                );
            }

            return (0, 0, 0, 0, 0, 0, 0, 0);
        }

        // Mapear reporte desde reader
        private static Reporte MapearReporte(Microsoft.Data.Sqlite.SqliteDataReader reader)
        {
            return new Reporte
            {
                Id = reader.GetInt32(0),
                TurnoId = reader.GetInt32(1),
                NombreOperador = reader.GetString(2),
                TipoTurno = reader.GetString(3),
                Fecha = reader.GetString(4),
                HoraApertura = reader.GetString(5),
                HoraCierre = reader.IsDBNull(6) ? "—" : reader.GetString(6),
                IngresoMaquinas = reader.GetDouble(7),
                TotalProductos = reader.GetDouble(8),
                TotalBruto = reader.GetDouble(9),
                Descuentos = reader.GetDouble(10),
                TotalLiquido = reader.GetDouble(11),
                MontoYape = reader.GetDouble(12),
                Efectivo = reader.GetDouble(13),
                ImagenPanCafe1 = reader.IsDBNull(14) ? null : reader.GetString(14),
                ImagenPanCafe2 = reader.IsDBNull(15) ? null : reader.GetString(15),
                ImagenYape = reader.IsDBNull(16) ? null : reader.GetString(16),
                PdfPath = reader.IsDBNull(17) ? null : reader.GetString(17),
                CreadoEn = reader.GetString(18)
            };
        }
    }
}