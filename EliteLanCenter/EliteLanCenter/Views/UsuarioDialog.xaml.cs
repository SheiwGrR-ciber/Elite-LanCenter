// =============================================
// ELITE LAN CENTER - USUARIO DIALOG
// =============================================

using EliteLanCenter.Controllers;
using EliteLanCenter.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EliteLanCenter.Views
{
    public partial class UsuarioDialog : Window
    {
        private readonly int? _usuarioId;
        private readonly bool _esNuevo;

        public UsuarioDialog()
        {
            InitializeComponent();
            _usuarioId = null;
            _esNuevo = true;
            LblTitulo.Text = "Nuevo Usuario";
            TxtNombre.Focus();
        }

        public UsuarioDialog(Usuario usuario) : this()
        {
            _usuarioId = usuario.Id;
            _esNuevo = false;
            LblTitulo.Text = "Editar Usuario";
            TxtNombre.Text = usuario.Nombre;
            TxtUserName.Text = usuario.UserName;

            foreach (ComboBoxItem item in CmbRol.Items)
            {
                if (item.Tag?.ToString() == usuario.Rol)
                {
                    item.IsSelected = true;
                    break;
                }
            }

            LblContrasenaTexto.Text = "Nueva contraseña (opcional)";
            LblReq1.Visibility = Visibility.Collapsed;
            LblReq2.Visibility = Visibility.Collapsed;
            LblReq3.Visibility = Visibility.Collapsed;
            LblReq4.Visibility = Visibility.Collapsed;
        }

        public (string nombre, string userName, string contrasena, string rol) ObtenerDatos()
        {
            var rolItem = CmbRol.SelectedItem as ComboBoxItem;
            return (
                TxtNombre.Text.Trim(),
                TxtUserName.Text.Trim(),
                TxtContrasena.Password,
                rolItem?.Tag?.ToString() ?? "Operador"
            );
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var nombre = TxtNombre.Text.Trim();
            var userName = TxtUserName.Text.Trim();
            var contrasena = TxtContrasena.Password;
            var rolItem = CmbRol.SelectedItem as ComboBoxItem;
            var rol = rolItem?.Tag?.ToString() ?? "Operador";

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MostrarError("El nombre es obligatorio.");
                TxtNombre.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                MostrarError("El nombre de usuario es obligatorio.");
                TxtUserName.Focus();
                return;
            }

            if (_esNuevo)
            {
                if (!ValidarContrasena(contrasena))
                    return;

                var (ok, mensaje) = UsuarioController.Crear(nombre, userName, contrasena, rol);
                if (!ok)
                {
                    MostrarError(mensaje);
                    return;
                }
            }
            else
            {
                var (ok, mensaje) = UsuarioController.Actualizar(_usuarioId!.Value, nombre, userName, rol);
                if (!ok)
                {
                    MostrarError(mensaje);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(contrasena))
                {
                    if (!ValidarContrasena(contrasena))
                        return;

                    var (_, msgContrasena) = UsuarioController.CambiarContrasena(_usuarioId!.Value, contrasena);
                }
            }

            DialogResult = true;
            Close();
        }

        private bool ValidarContrasena(string contrasena)
        {
            var colorOk = new SolidColorBrush(Color.FromRgb(0x01, 0xEF, 0xAC));
            var colorError = new SolidColorBrush(Color.FromRgb(0x89, 0x92, 0xB0));

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

            if (!ok1 || !ok2 || !ok3 || !ok4)
            {
                MostrarError("La contraseña no cumple los requisitos.");
                return false;
            }

            return true;
        }

        private void MostrarError(string mensaje)
        {
            LblError.Text = mensaje;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
