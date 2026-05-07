// =============================================
// ELITE LAN CENTER - CONTROLLER DE USUARIOS
// =============================================

using EliteLanCenter.Database;
using EliteLanCenter.Models;
using Microsoft.Data.Sqlite;

namespace EliteLanCenter.Controllers
{
    public class UsuarioController
    {
        // Verificar credenciales al hacer login
        public static Usuario? Login(string userName, string contrasena)
        {
            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT Id, Nombre, Usuario, Contrasena, Rol, Activo
                FROM Usuarios
                WHERE Usuario = @usuario 
                AND Contrasena = @contrasena
                AND Activo = 1
            ";

            command.Parameters.AddWithValue("@usuario", userName);
            command.Parameters.AddWithValue("@contrasena", contrasena);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Usuario
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    UserName = reader.GetString(2),
                    Contrasena = reader.GetString(3),
                    Rol = reader.GetString(4),
                    Activo = reader.GetInt32(5) == 1
                };
            }

            return null;
        }

        // Obtener todos los usuarios
        public static List<Usuario> ObtenerTodos()
        {
            var usuarios = new List<Usuario>();

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT Id, Nombre, Usuario, Rol, Activo, CreadoEn
                FROM Usuarios
                ORDER BY Nombre ASC
            ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                usuarios.Add(new Usuario
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    UserName = reader.GetString(2),
                    Rol = reader.GetString(3),
                    Activo = reader.GetInt32(4) == 1,
                    CreadoEn = reader.GetString(5)
                });
            }

            return usuarios;
        }

        // Crear nuevo usuario
        public static (bool ok, string mensaje) Crear(string nombre, string userName,
                                                       string contrasena, string rol)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(userName))
                return (false, "El usuario es obligatorio.");

            if (!ValidarContrasenaRobusta(contrasena))
                return (false, "La contraseña debe tener mínimo 8 caracteres, una mayúscula, una minúscula y un número.");

            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                command.CommandText = @"
                    INSERT INTO Usuarios (Nombre, Usuario, Contrasena, Rol)
                    VALUES (@nombre, @usuario, @contrasena, @rol)
                ";

                command.Parameters.AddWithValue("@nombre", nombre.Trim());
                command.Parameters.AddWithValue("@usuario", userName.Trim());
                command.Parameters.AddWithValue("@contrasena", contrasena);
                command.Parameters.AddWithValue("@rol", rol);

                command.ExecuteNonQuery();
                return (true, "Usuario creado correctamente.");
            }
            catch (SqliteException ex) when (ex.Message.Contains("UNIQUE"))
            {
                return (false, "El nombre de usuario ya existe.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // Cambiar contraseña
        public static (bool ok, string mensaje) CambiarContrasena(int usuarioId,
                                                                    string contrasenaNueva)
        {
            if (!ValidarContrasenaRobusta(contrasenaNueva))
                return (false, "La contraseña debe tener mínimo 8 caracteres, una mayúscula, una minúscula y un número.");

            using var connection = DatabaseConnection.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                UPDATE Usuarios SET Contrasena = @contrasena
                WHERE Id = @id
            ";

            command.Parameters.AddWithValue("@contrasena", contrasenaNueva);
            command.Parameters.AddWithValue("@id", usuarioId);

            command.ExecuteNonQuery();
            return (true, "Contraseña actualizada correctamente.");
        }

        // Eliminar usuario
        public static (bool ok, string mensaje) Eliminar(int usuarioId)
        {
            try
            {
                using var connection = DatabaseConnection.GetConnection();
                using var command = connection.CreateCommand();

                command.CommandText = "DELETE FROM Usuarios WHERE Id = @id";
                command.Parameters.AddWithValue("@id", usuarioId);
                command.ExecuteNonQuery();

                return (true, "Usuario eliminado correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // Validar contraseña robusta
        public static bool ValidarContrasenaRobusta(string contrasena)
        {
            if (contrasena.Length < 8) return false;
            if (!contrasena.Any(char.IsUpper)) return false;
            if (!contrasena.Any(char.IsLower)) return false;
            if (!contrasena.Any(char.IsDigit)) return false;
            return true;
        }
    }
}