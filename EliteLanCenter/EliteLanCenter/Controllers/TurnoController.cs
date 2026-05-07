// =============================================
// ELITE LAN CENTER - CONTROLLER DE TURNOS
// =============================================

using EliteLanCenter.Database;
using EliteLanCenter.Models;

namespace EliteLanCenter.Controllers
{
    public class TurnoController
    {
        // Abrir un nuevo turno
        public static (bool ok, string mensaje, int turnoId) AbrirTurno(int usuarioId, string tipo)
        {
            // Verificar si ya hay un turno abierto
            var turnoActivo = ObtenerTurnoAbierto();
            if (turnoActivo != null)
                return (false, "Ya hay un turno abierto. Ciérralo primero.", 0);

            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                var fecha = DateTime.Now.ToString("yyyy-MM-dd");

                command.CommandText = @"
                    INSERT INTO Turnos (UsuarioId, Tipo, Fecha, Abierto)
                    VALUES (@usuarioId, @tipo, @fecha, 1);
                    SELECT last_insert_rowid();
                ";

                command.Parameters.AddWithValue("@usuarioId", usuarioId);
                command.Parameters.AddWithValue("@tipo", tipo);
                command.Parameters.AddWithValue("@fecha", fecha);

                var turnoId = (int)(long)command.ExecuteScalar()!;
                return (true, $"Turno {tipo} abierto correctamente.", turnoId);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", 0);
            }
        }

        // Cerrar turno activo
        public static (bool ok, string mensaje) CerrarTurno(int turnoId)
        {
            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                var cerradaEn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                command.CommandText = @"
                    UPDATE Turnos
                    SET Abierto = 0, CerradaEn = @cerradaEn
                    WHERE Id = @id
                ";

                command.Parameters.AddWithValue("@cerradaEn", cerradaEn);
                command.Parameters.AddWithValue("@id", turnoId);

                command.ExecuteNonQuery();
                return (true, "Turno cerrado correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // Obtener turno actualmente abierto
        public static Turno? ObtenerTurnoAbierto()
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT 
                    t.Id, t.UsuarioId, u.Nombre,
                    t.Tipo, t.Fecha, t.Abierto,
                    t.AbiertaEn, t.CerradaEn
                FROM Turnos t
                JOIN Usuarios u ON u.Id = t.UsuarioId
                WHERE t.Abierto = 1
                LIMIT 1
            ";

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Turno
                {
                    Id = reader.GetInt32(0),
                    UsuarioId = reader.GetInt32(1),
                    NombreOperador = reader.GetString(2),
                    Tipo = reader.GetString(3),
                    Fecha = reader.GetString(4),
                    Abierto = reader.GetInt32(5) == 1,
                    AbiertaEn = reader.GetString(6),
                    CerradaEn = reader.IsDBNull(7) ? null : reader.GetString(7)
                };
            }

            return null;
        }

        // Obtener turnos de la semana actual
        public static List<Turno> ObtenerTurnosSemanaActual()
        {
            var turnos = new List<Turno>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            // Calcular lunes y domingo de la semana actual
            var hoy = DateTime.Now.Date;
            var lunes = hoy.AddDays(-(int)hoy.DayOfWeek + (int)DayOfWeek.Monday);
            var domingo = lunes.AddDays(6);

            command.CommandText = @"
                SELECT 
                    t.Id, t.UsuarioId, u.Nombre,
                    t.Tipo, t.Fecha, t.Abierto,
                    t.AbiertaEn, t.CerradaEn
                FROM Turnos t
                JOIN Usuarios u ON u.Id = t.UsuarioId
                WHERE t.Fecha BETWEEN @lunes AND @domingo
                ORDER BY t.Fecha ASC,
                    CASE t.Tipo
                        WHEN 'Mañana' THEN 1
                        WHEN 'Tarde'  THEN 2
                        WHEN 'Noche'  THEN 3
                    END
            ";

            command.Parameters.AddWithValue("@lunes", lunes.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@domingo", domingo.ToString("yyyy-MM-dd"));

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                turnos.Add(new Turno
                {
                    Id = reader.GetInt32(0),
                    UsuarioId = reader.GetInt32(1),
                    NombreOperador = reader.GetString(2),
                    Tipo = reader.GetString(3),
                    Fecha = reader.GetString(4),
                    Abierto = reader.GetInt32(5) == 1,
                    AbiertaEn = reader.GetString(6),
                    CerradaEn = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }

            return turnos;
        }

        // Registrar conteo de stock al cerrar turno
        public static (bool ok, string mensaje) RegistrarConteoTurno(int turnoId,
                                                   List<(int productoId, int cantidad)> conteos)
        {
            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                foreach (var (productoId, cantidad) in conteos)
                {
                    command.CommandText = @"
                        INSERT INTO ConteoTurno (TurnoId, ProductoId, Cantidad)
                        VALUES (@turnoId, @productoId, @cantidad)
                    ";

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@turnoId", turnoId);
                    command.Parameters.AddWithValue("@productoId", productoId);
                    command.Parameters.AddWithValue("@cantidad", cantidad);
                    command.ExecuteNonQuery();
                }

                return (true, "Conteo registrado correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // Sugerir tipo de turno según la hora actual
        public static string SugerirTipoTurno()
        {
            var hora = DateTime.Now.Hour;
            return hora switch
            {
                >= 6 and < 14 => "Mañana",
                >= 14 and < 22 => "Tarde",
                _ => "Noche"
            };
        }
    }
}