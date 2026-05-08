// =============================================
// ELITE LAN CENTER - LOGIN VIEW CODE BEHIND
// =============================================

using EliteLanCenter.Controllers;
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
            var usuario = TxtUsuario.Password != null ? TxtUsuario.Text.Trim() : TxtUsuario.Text.Trim();
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