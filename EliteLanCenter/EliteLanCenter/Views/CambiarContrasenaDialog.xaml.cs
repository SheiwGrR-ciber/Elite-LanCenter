// =============================================
// ELITE LAN CENTER - CAMBIAR CONTRASEÑA DIALOG
// =============================================

using EliteLanCenter.Controllers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EliteLanCenter.Views
{
    public partial class CambiarContrasenaDialog : Window
    {
        private readonly string _nombreUsuario;

        public CambiarContrasenaDialog(string nombreUsuario)
        {
            InitializeComponent();
            _nombreUsuario = nombreUsuario;
            LblUsuario.Text = $"Usuario: {nombreUsuario}";
            TxtContrasena.Focus();
        }

        public string ObtenerContrasena()
        {
            return TxtContrasena.Password;
        }

        private void TxtContrasena_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ActualizarValidacion(TxtContrasena.Password);
        }

        private void ActualizarValidacion(string contrasena)
        {
            var colorOk = new SolidColorBrush(Color.FromRgb(0x01, 0xEF, 0xAC));
            var colorError = new SolidColorBrush(Color.FromRgb(0x89, 0x92, 0xB0));
            var colorWeak = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
            var colorMedium = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));
            var colorStrong = new SolidColorBrush(Color.FromRgb(0x01, 0xEF, 0xAC));

            bool ok1 = contrasena.Length >= 8;
            bool ok2 = contrasena.Any(char.IsUpper);
            bool ok3 = contrasena.Any(char.IsLower);
            bool ok4 = contrasena.Any(char.IsDigit);

            LblReq1.Foreground = ok1 ? colorOk : colorError;
            LblReq2.Foreground = ok2 ? colorOk : colorError;
            LblReq3.Foreground = ok3 ? colorOk : colorError;
            LblReq4.Foreground = ok4 ? colorOk : colorError;

            LblReq1.Text = (ok1 ? "✓" : "✗") + " Mínimo 8 caracteres";
            LblReq2.Text = (ok2 ? "✓" : "✗") + " Al menos una mayúscula";
            LblReq3.Text = (ok3 ? "✓" : "✗") + " Al menos una minúscula";
            LblReq4.Text = (ok4 ? "✓" : "✗") + " Al menos un número";

            int score = 0;
            if (ok1) score++;
            if (ok2) score++;
            if (ok3) score++;
            if (ok4) score++;

            double progress = (score / 4.0) * 100;
            BarraProgreso.Width = (progress / 100.0) * (ActualWidth - 64);

            if (score <= 1)
            {
                BarraProgreso.Background = colorWeak;
                LblFortaleza.Text = "Fortaleza: Débil";
                LblFortaleza.Foreground = colorWeak;
            }
            else if (score <= 3)
            {
                BarraProgreso.Background = colorMedium;
                LblFortaleza.Text = "Fortaleza: Media";
                LblFortaleza.Foreground = colorMedium;
            }
            else
            {
                BarraProgreso.Background = colorStrong;
                LblFortaleza.Text = "Fortaleza: Fuerte";
                LblFortaleza.Foreground = colorStrong;
            }
        }

        private void BtnCambiar_Click(object sender, RoutedEventArgs e)
        {
            var contrasena = TxtContrasena.Password;
            var confirmar = TxtConfirmar.Password;

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                LblError.Text = "La contraseña es obligatoria.";
                TxtContrasena.Focus();
                return;
            }

            if (!UsuarioController.ValidarContrasenaRobusta(contrasena))
            {
                LblError.Text = "La contraseña no cumple los requisitos.";
                return;
            }

            if (contrasena != confirmar)
            {
                LblError.Text = "Las contraseñas no coinciden.";
                TxtConfirmar.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
