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
        public static List<(string fecha, double maquinas, double productos)>
            ObtenerEstadisticasSemana(string fechaInicio, string fechaFin)
        {
            var estadisticas = new List<(string, double, double)>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 
                    t.Fecha,
                    COALESCE(SUM(r.IngresoMaquinas), 0) AS Maquinas,
                    COALESCE(SUM(r.TotalProductos), 0)  AS Productos
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
                    reader.GetDouble(2)
                ));
            }

            return estadisticas;
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