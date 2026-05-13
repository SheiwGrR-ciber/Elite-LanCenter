// =============================================
// ELITE LAN CENTER - CONTROLLER DE PRODUCTOS
// =============================================

using EliteLanCenter.Database;
using EliteLanCenter.Models;
using Microsoft.Data.Sqlite;

namespace EliteLanCenter.Controllers
{
    public class ProductoController
    {
        // Obtener todos los productos activos con su stock
        public static List<Producto> ObtenerTodos()
        {
            var productos = new List<Producto>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 
                    p.Id,
                    p.Nombre,
                    p.PrecioUnidad,
                    p.PrecioPaquete,
                    p.UnidadesPaquete,
                    COALESCE(sm.Cantidad, 0)        AS StockMostrador,
                    COALESCE(sa.Paquetes, 0)        AS PaquetesAlmacen,
                    COALESCE(sa.UnidadesSueltas, 0) AS UnidadesSueltas
                FROM Productos p
                LEFT JOIN StockMostrador sm ON sm.ProductoId = p.Id
                LEFT JOIN StockAlmacen   sa ON sa.ProductoId = p.Id
                WHERE p.Activo = 1
                ORDER BY p.Nombre ASC
            ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                productos.Add(new Producto
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    PrecioUnidad = reader.GetDouble(2),
                    PrecioPaquete = reader.GetDouble(3),
                    UnidadesPaquete = reader.GetInt32(4),
                    StockMostrador = reader.GetInt32(5),
                    PaquetesAlmacen = reader.GetInt32(6),
                    UnidadesSueltas = reader.GetInt32(7)
                });
            }

            return productos;
        }

        // Obtener producto por ID
        public static Producto? ObtenerPorId(int id)
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 
                    p.Id, p.Nombre, p.PrecioUnidad, p.PrecioPaquete,
                    p.UnidadesPaquete,
                    COALESCE(sm.Cantidad, 0),
                    COALESCE(sa.Paquetes, 0),
                    COALESCE(sa.UnidadesSueltas, 0)
                FROM Productos p
                LEFT JOIN StockMostrador sm ON sm.ProductoId = p.Id
                LEFT JOIN StockAlmacen   sa ON sa.ProductoId = p.Id
                WHERE p.Id = @id AND p.Activo = 1
            ";

            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Producto
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    PrecioUnidad = reader.GetDouble(2),
                    PrecioPaquete = reader.GetDouble(3),
                    UnidadesPaquete = reader.GetInt32(4),
                    StockMostrador = reader.GetInt32(5),
                    PaquetesAlmacen = reader.GetInt32(6),
                    UnidadesSueltas = reader.GetInt32(7)
                };
            }

            return null;
        }

        // Agregar nuevo producto
        public static (bool ok, string mensaje) Agregar(string nombre, double precioUnidad,
                                                         double precioPaquete, int unidadesPaquete)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre es obligatorio.");

            if (precioUnidad <= 0)
                return (false, "El precio unitario debe ser mayor a 0.");

            if (unidadesPaquete <= 0)
                return (false, "Las unidades por paquete deben ser mayor a 0.");

            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                command.CommandText = @"
                    INSERT INTO Productos (Nombre, PrecioUnidad, PrecioPaquete, UnidadesPaquete)
                    VALUES (@nombre, @precioUnidad, @precioPaquete, @unidadesPaquete);
                    SELECT last_insert_rowid();
                ";

                command.Parameters.AddWithValue("@nombre", nombre.Trim());
                command.Parameters.AddWithValue("@precioUnidad", precioUnidad);
                command.Parameters.AddWithValue("@precioPaquete", precioPaquete);
                command.Parameters.AddWithValue("@unidadesPaquete", unidadesPaquete);

                var productoId = (long)command.ExecuteScalar()!;

                // Crear registros de stock en 0
                command.CommandText = @"
                    INSERT INTO StockMostrador (ProductoId, Cantidad) VALUES (@id, 0);
                    INSERT INTO StockAlmacen (ProductoId, Paquetes, UnidadesSueltas) VALUES (@id, 0, 0);
                ";
                command.Parameters.AddWithValue("@id", productoId);
                command.ExecuteNonQuery();

                return (true, "Producto agregado correctamente.");
            }
            catch (SqliteException ex) when (ex.Message.Contains("UNIQUE"))
            {
                return (false, "Ya existe un producto con ese nombre.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // Actualizar producto
        public static (bool ok, string mensaje) Actualizar(int id, string nombre,
                                                            double precioUnidad,
                                                            double precioPaquete,
                                                            int unidadesPaquete)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre es obligatorio.");

            if (precioUnidad <= 0)
                return (false, "El precio unitario debe ser mayor a 0.");

            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                command.CommandText = @"
                    UPDATE Productos
                    SET Nombre = @nombre,
                        PrecioUnidad = @precioUnidad,
                        PrecioPaquete = @precioPaquete,
                        UnidadesPaquete = @unidadesPaquete
                    WHERE Id = @id
                ";

                command.Parameters.AddWithValue("@nombre", nombre.Trim());
                command.Parameters.AddWithValue("@precioUnidad", precioUnidad);
                command.Parameters.AddWithValue("@precioPaquete", precioPaquete);
                command.Parameters.AddWithValue("@unidadesPaquete", unidadesPaquete);
                command.Parameters.AddWithValue("@id", id);

                command.ExecuteNonQuery();
                return (true, "Producto actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // Desactivar producto
        public static (bool ok, string mensaje) Desactivar(int id)
        {
            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                command.CommandText = "UPDATE Productos SET Activo = 0 WHERE Id = @id";
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();

                return (true, "Producto eliminado correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // Actualizar stock mostrador
        public static void ActualizarStockMostrador(int productoId, int cantidad)
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                UPDATE StockMostrador
                SET Cantidad = @cantidad,
                    ActualizadoEn = datetime('now', 'localtime')
                WHERE ProductoId = @productoId
            ";

            command.Parameters.AddWithValue("@cantidad", cantidad);
            command.Parameters.AddWithValue("@productoId", productoId);
            command.ExecuteNonQuery();
        }

        // Actualizar stock almacen
        public static void ActualizarStockAlmacen(int productoId, int paquetes,
                                                   int unidadesSueltas)
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                UPDATE StockAlmacen
                SET Paquetes = @paquetes,
                    UnidadesSueltas = @unidadesSueltas,
                    ActualizadoEn = datetime('now', 'localtime')
                WHERE ProductoId = @productoId
            ";

            command.Parameters.AddWithValue("@paquetes", paquetes);
            command.Parameters.AddWithValue("@unidadesSueltas", unidadesSueltas);
            command.Parameters.AddWithValue("@productoId", productoId);
            command.ExecuteNonQuery();
        }

        // Ingresar mercadería al almacén
        public static (bool ok, string mensaje) IngresarMercaderia(
            int productoId, int paquetes, int unidadesSueltas,
            double costoTotal, int usuarioId)
        {
            if (paquetes < 0 || unidadesSueltas < 0)
                return (false, "Las cantidades no pueden ser negativas.");

            if (paquetes == 0 && unidadesSueltas == 0)
                return (false, "Debes ingresar al menos 1 paquete o unidad suelta.");

            if (costoTotal < 0)
                return (false, "El costo total no puede ser negativo.");

            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                // Obtener stock actual
                command.CommandText = @"
                    SELECT Paquetes, UnidadesSueltas
                    FROM StockAlmacen
                    WHERE ProductoId = @productoId
                ";
                command.Parameters.AddWithValue("@productoId", productoId);

                int paqActual = 0, undActual = 0;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        paqActual = reader.GetInt32(0);
                        undActual = reader.GetInt32(1);
                    }
                }

                var nuevoPaquetes = paqActual + paquetes;
                var nuevoUnidades = undActual + unidadesSueltas;

                // Actualizar stock almacén
                command.CommandText = @"
                    UPDATE StockAlmacen
                    SET Paquetes = @paquetes,
                        UnidadesSueltas = @unidadesSueltas,
                        ActualizadoEn = datetime('now', 'localtime')
                    WHERE ProductoId = @productoId
                ";
                command.Parameters.AddWithValue("@paquetes", nuevoPaquetes);
                command.Parameters.AddWithValue("@unidadesSueltas", nuevoUnidades);
                command.ExecuteNonQuery();

                // Registrar ingreso
                command.CommandText = @"
                    INSERT INTO IngresosMercaderia (ProductoId, Cantidad, EsPaquete, CostoTotal, RegistradoPor)
                    VALUES (@pid, @cant, @esPaq, @costo, @uid)
                ";
                command.Parameters.AddWithValue("@pid", productoId);
                command.Parameters.AddWithValue("@uid", usuarioId);

                if (paquetes > 0)
                {
                    command.Parameters.AddWithValue("@cant", paquetes);
                    command.Parameters.AddWithValue("@esPaq", 1);
                    command.Parameters.AddWithValue("@costo", Math.Round(costoTotal * paquetes / (paquetes + unidadesSueltas), 2));
                    command.ExecuteNonQuery();
                }

                if (unidadesSueltas > 0)
                {
                    command.Parameters.AddWithValue("@cant", unidadesSueltas);
                    command.Parameters.AddWithValue("@esPaq", 0);
                    command.Parameters.AddWithValue("@costo", Math.Round(costoTotal * unidadesSueltas / (paquetes + unidadesSueltas), 2));
                    command.ExecuteNonQuery();
                }

                return (true, $"Ingreso registrado: {paquetes} paquete(s) y {unidadesSueltas} unidad(es) suelta(s).");
            }
            catch (Exception ex)
            {
                return (false, $"Error al ingresar mercadería: {ex.Message}");
            }
        }

        // Calcular valor total del inventario
        public static double CalcularValorTotalInventario()
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT COALESCE(SUM(
                    (COALESCE(sm.Cantidad, 0) +
                     COALESCE(sa.Paquetes, 0) * p.UnidadesPaquete +
                     COALESCE(sa.UnidadesSueltas, 0)) * p.PrecioUnidad
                ), 0)
                FROM Productos p
                LEFT JOIN StockMostrador sm ON sm.ProductoId = p.Id
                LEFT JOIN StockAlmacen   sa ON sa.ProductoId = p.Id
                WHERE p.Activo = 1
            ";

            return Convert.ToDouble(command.ExecuteScalar() ?? 0.0);
        }
    }
}