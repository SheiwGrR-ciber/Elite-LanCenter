// =============================================
// ELITE LAN CENTER - CONTROLLER DE CASEROS
// =============================================

using EliteLanCenter.Database;
using EliteLanCenter.Models;
using Microsoft.Data.Sqlite;

namespace EliteLanCenter.Controllers
{
    public class CaseroController
    {
        // Obtener todos los caseros
        public static List<Casero> ObtenerTodos()
        {
            var caseros = new List<Casero>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT Id, Nombre, Apodo, Telefono,
                       CodigoRFID, Saldo, Activo,
                       UltimaVisita, CreadoEn
                FROM Caseros
                ORDER BY Nombre ASC
            ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                caseros.Add(new Casero
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Apodo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Telefono = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    CodigoRFID = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Saldo = reader.GetDouble(5),
                    Activo = reader.GetInt32(6) == 1,
                    UltimaVisita = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    CreadoEn = reader.GetString(8)
                });
            }

            return caseros;
        }

        // Buscar casero por código RFID
        public static Casero? BuscarPorRFID(string codigoRFID)
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT Id, Nombre, Apodo, Telefono,
                       CodigoRFID, Saldo, Activo,
                       UltimaVisita, CreadoEn
                FROM Caseros
                WHERE CodigoRFID = @codigo AND Activo = 1
            ";

            command.Parameters.AddWithValue("@codigo", codigoRFID);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Casero
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Apodo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Telefono = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    CodigoRFID = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Saldo = reader.GetDouble(5),
                    Activo = reader.GetInt32(6) == 1,
                    UltimaVisita = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    CreadoEn = reader.GetString(8)
                };
            }

            return null;
        }

        // Buscar casero por nombre o apodo
        public static List<Casero> BuscarPorNombre(string busqueda)
        {
            var caseros = new List<Casero>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT Id, Nombre, Apodo, Telefono,
                       CodigoRFID, Saldo, Activo,
                       UltimaVisita, CreadoEn
                FROM Caseros
                WHERE (Nombre LIKE @busqueda OR Apodo LIKE @busqueda)
                AND Activo = 1
                ORDER BY Nombre ASC
            ";

            command.Parameters.AddWithValue("@busqueda", $"%{busqueda}%");

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                caseros.Add(new Casero
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Apodo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Telefono = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    CodigoRFID = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Saldo = reader.GetDouble(5),
                    Activo = reader.GetInt32(6) == 1,
                    UltimaVisita = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    CreadoEn = reader.GetString(8)
                });
            }

            return caseros;
        }

        // Registrar nuevo casero
        public static (bool ok, string mensaje) Registrar(string nombre, string apodo,
                                                           string telefono, string codigoRFID)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre es obligatorio.");

            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                command.CommandText = @"
                    INSERT INTO Caseros (Nombre, Apodo, Telefono, CodigoRFID, Saldo)
                    VALUES (@nombre, @apodo, @telefono, @rfid, 0)
                ";

                command.Parameters.AddWithValue("@nombre", nombre.Trim());
                command.Parameters.AddWithValue("@apodo", apodo.Trim());
                command.Parameters.AddWithValue("@telefono", telefono.Trim());
                command.Parameters.AddWithValue("@rfid", codigoRFID.Trim());

                command.ExecuteNonQuery();
                return (true, "Casero registrado correctamente.");
            }
            catch (SqliteException ex) when (ex.Message.Contains("UNIQUE"))
            {
                return (false, "Ya existe un casero con ese código RFID.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // Recargar saldo
        public static (bool ok, string mensaje) RecargarSaldo(int caseroId,
                                                               double monto, int usuarioId)
        {
            if (monto <= 0)
                return (false, "El monto debe ser mayor a 0.");

            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                // Actualizar saldo
                command.CommandText = @"
                    UPDATE Caseros
                    SET Saldo = Saldo + @monto,
                        UltimaVisita = datetime('now', 'localtime')
                    WHERE Id = @id
                ";

                command.Parameters.AddWithValue("@monto", monto);
                command.Parameters.AddWithValue("@id", caseroId);
                command.ExecuteNonQuery();

                // Registrar recarga
                command.CommandText = @"
                    INSERT INTO Recargas (CaseroId, Monto, UsuarioId)
                    VALUES (@caseroId, @monto, @usuarioId)
                ";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@caseroId", caseroId);
                command.Parameters.AddWithValue("@monto", monto);
                command.Parameters.AddWithValue("@usuarioId", usuarioId);
                command.ExecuteNonQuery();

                return (true, $"Recarga de S/ {monto:F2} realizada correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // Registrar consumo
        public static (bool ok, string mensaje) RegistrarConsumo(int caseroId,
                                                                   double monto, int usuarioId)
        {
            if (monto <= 0)
                return (false, "El monto debe ser mayor a 0.");

            // Obtener saldo actual
            var casero = ObtenerPorId(caseroId);
            if (casero == null)
                return (false, "Casero no encontrado.");

            if (casero.Saldo < monto)
                return (false, $"Saldo insuficiente. Saldo actual: S/ {casero.Saldo:F2}");

            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                var saldoAnterior = casero.Saldo;
                var saldoNuevo = Math.Round(casero.Saldo - monto, 2);

                // Descontar saldo
                command.CommandText = @"
                    UPDATE Caseros
                    SET Saldo = @saldoNuevo,
                        UltimaVisita = datetime('now', 'localtime')
                    WHERE Id = @id
                ";

                command.Parameters.AddWithValue("@saldoNuevo", saldoNuevo);
                command.Parameters.AddWithValue("@id", caseroId);
                command.ExecuteNonQuery();

                // Registrar consumo
                command.CommandText = @"
                    INSERT INTO Consumos (CaseroId, Monto, SaldoAnterior, SaldoNuevo, UsuarioId)
                    VALUES (@caseroId, @monto, @saldoAnterior, @saldoNuevo, @usuarioId)
                ";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@caseroId", caseroId);
                command.Parameters.AddWithValue("@monto", monto);
                command.Parameters.AddWithValue("@saldoAnterior", saldoAnterior);
                command.Parameters.AddWithValue("@saldoNuevo", saldoNuevo);
                command.Parameters.AddWithValue("@usuarioId", usuarioId);
                command.ExecuteNonQuery();

                // Verificar saldo bajo
                var mensaje = saldoNuevo < 2.00
                    ? $"Consumo registrado. ⚠️ Saldo bajo: S/ {saldoNuevo:F2}"
                    : $"Consumo registrado. Saldo restante: S/ {saldoNuevo:F2}";

                return (true, mensaje);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // Obtener casero por ID
        public static Casero? ObtenerPorId(int id)
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT Id, Nombre, Apodo, Telefono,
                       CodigoRFID, Saldo, Activo,
                       UltimaVisita, CreadoEn
                FROM Caseros WHERE Id = @id
            ";

            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Casero
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Apodo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Telefono = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    CodigoRFID = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Saldo = reader.GetDouble(5),
                    Activo = reader.GetInt32(6) == 1,
                    UltimaVisita = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    CreadoEn = reader.GetString(8)
                };
            }

            return null;
        }

        // Eliminar casero
        public static (bool ok, string mensaje) Eliminar(int id)
        {
            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                command.CommandText = "UPDATE Caseros SET Activo = 0 WHERE Id = @id";
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();

                return (true, "Casero eliminado correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // Obtener historial de movimientos
        public static List<Movimiento> ObtenerHistorial(int caseroId)
        {
            var movimientos = new List<Movimiento>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 'Recarga' AS Tipo, r.Monto, 0 AS SaldoAnterior,
                       0 AS SaldoNuevo, u.Nombre, r.CreadoEn
                FROM Recargas r
                JOIN Usuarios u ON u.Id = r.UsuarioId
                WHERE r.CaseroId = @caseroId

                UNION ALL

                SELECT 'Consumo' AS Tipo, c.Monto, c.SaldoAnterior,
                       c.SaldoNuevo, u.Nombre, c.CreadoEn
                FROM Consumos c
                JOIN Usuarios u ON u.Id = c.UsuarioId
                WHERE c.CaseroId = @caseroId

                ORDER BY CreadoEn DESC
            ";

            command.Parameters.AddWithValue("@caseroId", caseroId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                movimientos.Add(new Movimiento
                {
                    Tipo = reader.GetString(0),
                    Monto = reader.GetDouble(1),
                    SaldoAnterior = reader.GetDouble(2),
                    SaldoNuevo = reader.GetDouble(3),
                    NombreUsuario = reader.GetString(4),
                    CreadoEn = reader.GetString(5)
                });
            }

            return movimientos;
        }
    }
}