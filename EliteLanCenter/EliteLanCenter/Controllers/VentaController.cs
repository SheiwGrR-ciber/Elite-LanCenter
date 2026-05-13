// =============================================
// ELITE LAN CENTER - CONTROLLER DE VENTAS
// =============================================

using EliteLanCenter.Database;
using EliteLanCenter.Models;

namespace EliteLanCenter.Controllers
{
    public class VentaController
    {
        // Registrar una venta
        public static (bool ok, string mensaje, double total) RegistrarVenta(
            int turnoId, int productoId, int cantidad, bool fiado = false, string? numeroPc = null)
        {
            // Obtener producto
            var producto = ProductoController.ObtenerPorId(productoId);
            if (producto == null)
                return (false, "Producto no encontrado.", 0);

            // Verificar stock
            if (producto.StockMostrador < cantidad)
                return (false, $"Stock insuficiente. Solo hay {producto.StockMostrador} unidades.", 0);

            var total = Math.Round(producto.PrecioUnidad * cantidad, 2);

            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                command.CommandText = @"
                    INSERT INTO Ventas (TurnoId, ProductoId, Cantidad, PrecioUnit, Total, Fiado, FiadoPagado, NumeroPc)
                    VALUES (@turnoId, @productoId, @cantidad, @precioUnit, @total, @fiado, 0, @numeroPc)
                ";

                command.Parameters.AddWithValue("@turnoId", turnoId);
                command.Parameters.AddWithValue("@productoId", productoId);
                command.Parameters.AddWithValue("@cantidad", cantidad);
                command.Parameters.AddWithValue("@precioUnit", producto.PrecioUnidad);
                command.Parameters.AddWithValue("@total", total);
                command.Parameters.AddWithValue("@fiado", fiado ? 1 : 0);
                command.Parameters.AddWithValue("@numeroPc", numeroPc ?? (object)DBNull.Value);

                command.ExecuteNonQuery();

                // Descontar del stock mostrador
                var nuevoStock = producto.StockMostrador - cantidad;
                ProductoController.ActualizarStockMostrador(productoId, nuevoStock);

                return (true, $"Venta registrada. Total: S/ {total:F2}", total);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", 0);
            }
        }

        // Obtener ventas del turno actual
        public static List<Venta> ObtenerVentasPorTurno(int turnoId)
        {
            var ventas = new List<Venta>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 
                    v.Id, v.TurnoId, v.ProductoId, p.Nombre,
                    v.Cantidad, v.PrecioUnit, v.Total,
                    v.Fiado, v.FiadoPagado, v.NumeroPc, v.CreadoEn
                FROM Ventas v
                JOIN Productos p ON p.Id = v.ProductoId
                WHERE v.TurnoId = @turnoId
                ORDER BY v.CreadoEn DESC
            ";

            command.Parameters.AddWithValue("@turnoId", turnoId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                ventas.Add(new Venta
                {
                    Id = reader.GetInt32(0),
                    TurnoId = reader.GetInt32(1),
                    ProductoId = reader.GetInt32(2),
                    NombreProducto = reader.GetString(3),
                    Cantidad = reader.GetInt32(4),
                    PrecioUnit = reader.GetDouble(5),
                    Total = reader.GetDouble(6),
                    Fiado = reader.GetInt32(7) == 1,
                    FiadoPagado = reader.GetInt32(8) == 1,
                    NumeroPc = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CreadoEn = reader.GetString(10)
                });
            }

            return ventas;
        }

        // Obtener total vendido en el turno
        public static double ObtenerTotalTurno(int turnoId)
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT COALESCE(SUM(Total), 0)
                FROM Ventas
                WHERE TurnoId = @turnoId
                AND (Fiado = 0 OR FiadoPagado = 1)
            ";

            command.Parameters.AddWithValue("@turnoId", turnoId);
            return Convert.ToDouble(command.ExecuteScalar() ?? 0.0);
        }

        // Obtener ventas del dia completo
        public static double ObtenerTotalDia(string fecha)
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT COALESCE(SUM(v.Total), 0)
                FROM Ventas v
                JOIN Turnos t ON t.Id = v.TurnoId
                WHERE t.Fecha = @fecha
                AND (v.Fiado = 0 OR v.FiadoPagado = 1)
            ";

            command.Parameters.AddWithValue("@fecha", fecha);
            return Convert.ToDouble(command.ExecuteScalar() ?? 0.0);
        }

        // Obtener fiados pendientes del turno
        public static List<Venta> ObtenerFiadosPendientes(int turnoId)
        {
            var ventas = new List<Venta>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 
                    v.Id, v.TurnoId, v.ProductoId, p.Nombre,
                    v.Cantidad, v.PrecioUnit, v.Total,
                    v.Fiado, v.FiadoPagado, v.NumeroPc, v.CreadoEn
                FROM Ventas v
                JOIN Productos p ON p.Id = v.ProductoId
                WHERE v.TurnoId = @turnoId
                AND v.Fiado = 1 AND v.FiadoPagado = 0
                ORDER BY v.CreadoEn ASC
            ";

            command.Parameters.AddWithValue("@turnoId", turnoId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                ventas.Add(new Venta
                {
                    Id = reader.GetInt32(0),
                    TurnoId = reader.GetInt32(1),
                    ProductoId = reader.GetInt32(2),
                    NombreProducto = reader.GetString(3),
                    Cantidad = reader.GetInt32(4),
                    PrecioUnit = reader.GetDouble(5),
                    Total = reader.GetDouble(6),
                    Fiado = reader.GetInt32(7) == 1,
                    FiadoPagado = reader.GetInt32(8) == 1,
                    NumeroPc = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CreadoEn = reader.GetString(10)
                });
            }

            return ventas;
        }

        // Marcar fiado como pagado
        public static (bool ok, string mensaje) PagarFiado(int ventaId)
        {
            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                command.CommandText = @"
                    UPDATE Ventas SET FiadoPagado = 1
                    WHERE Id = @id
                ";

                command.Parameters.AddWithValue("@id", ventaId);
                command.ExecuteNonQuery();

                return (true, "Fiado marcado como pagado.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // Resumen de ventas por producto en un turno
        public static List<(string producto, int cantidad, double total)> ResumenPorProducto(int turnoId)
        {
            var resumen = new List<(string, int, double)>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT p.Nombre, SUM(v.Cantidad), SUM(v.Total)
                FROM Ventas v
                JOIN Productos p ON p.Id = v.ProductoId
                WHERE v.TurnoId = @turnoId
                AND (v.Fiado = 0 OR v.FiadoPagado = 1)
                GROUP BY v.ProductoId
                ORDER BY SUM(v.Total) DESC
            ";

            command.Parameters.AddWithValue("@turnoId", turnoId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                resumen.Add((
                    reader.GetString(0),
                    reader.GetInt32(1),
                    reader.GetDouble(2)
                ));
            }

            return resumen;
        }
    }
}