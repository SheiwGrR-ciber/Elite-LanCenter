// =============================================
// ELITE LAN CENTER - LOGIN VIEW CODE BEHIND
// =============================================

using EliteLanCenter.Controllers;
using EliteLanCenter.Database;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EliteLanCenter.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            TxtUsuario.Focus();
        }

        // Botón ingresar
        private void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            IniciarSesion();
        }

        // Enter en campo usuario
        private void TxtUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                TxtContrasena.Focus();
        }

        // Enter en campo contraseña
        private void TxtContrasena_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                IniciarSesion();
        }

        // Lógica de login
        private void IniciarSesion()
        {
            var logFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "elite_login.log");
            System.IO.File.AppendAllText(logFile, $"[{DateTime.Now}] IniciarSesion called\r\n");

            // Inicializar base de datos ANTES del login
            try
            {
                System.IO.File.AppendAllText(logFile, $"[{DateTime.Now}] Calling Initialize...\r\n");
                DatabaseConnection.Initialize();
                DatabaseConnection.InsertarDatosIniciales();
                System.IO.File.AppendAllText(logFile, $"[{DateTime.Now}] Initialize complete\r\n");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(logFile, $"[{DateTime.Now}] ERROR: {ex.Message}\r\n{ex.StackTrace}\r\n");
                MessageBox.Show($"Error inicializando base de datos:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var usuario = TxtUsuario.Text.Trim();
            var contrasena = TxtContrasena.Password;

            // Validar campos vacíos
            if (string.IsNullOrWhiteSpace(usuario))
            {
                MostrarError("Ingresa tu nombre de usuario.");
                TxtUsuario.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                MostrarError("Ingresa tu contraseña.");
                TxtContrasena.Focus();
                return;
            }

            // Verificar credenciales
            var usuarioLogueado = UsuarioController.Login(usuario, contrasena);

            if (usuarioLogueado == null)
            {
                MostrarError("Usuario o contraseña incorrectos.");
                TxtContrasena.Clear();
                TxtContrasena.Focus();
                return;
            }

            // Login exitoso — abrir dashboard
            var dashboard = new DashboardView(usuarioLogueado);
            dashboard.Show();
            this.Close();
        }

        // Mostrar mensaje de error
        private void MostrarError(string mensaje)
        {
            LblError.Text = $"⚠ {mensaje}";
        }
    }
}