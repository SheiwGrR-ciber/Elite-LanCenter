// =============================================
// ELITE LAN CENTER - ADMINISTRACION PAGE
// =============================================

using EliteLanCenter.Controllers;
using EliteLanCenter.Models;
using System.Windows;
using System.Windows.Controls;

namespace EliteLanCenter.Pages
{
    public partial class AdministracionPage : Page
    {
        private readonly Usuario _usuario;
        private List<Usuario> _todosUsuarios = new();

        public AdministracionPage(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;

            if (!_usuario.EsAdmin)
            {
                MessageBox.Show("No tienes permisos para acceder a esta sección.",
                    "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                Visibility = Visibility.Collapsed;
                return;
            }

            CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            _todosUsuarios = UsuarioController.ObtenerTodos()
                .Select(u =>
                {
                    u.EsAdminDueno = u.Rol == "AdminDueno";
                    u.EsAdminEncargado = u.Rol == "AdminEncargado";
                    u.Iniciales = u.Nombre.Length >= 2
                        ? u.Nombre.Substring(0, 2).ToUpper()
                        : u.Nombre.ToUpper();
                    return u;
                })
                .ToList();

            ListaUsuarios.ItemsSource = _todosUsuarios;
            ActualizarEstadisticas();
        }

        private void ActualizarEstadisticas()
        {
            LblTotalUsuarios.Text = _todosUsuarios.Count.ToString();
            LblTotalAdmins.Text = _todosUsuarios.Count(u => u.EsAdmin).ToString();
            LblTotalOperadores.Text = _todosUsuarios.Count(u => !u.EsAdmin).ToString();
            LblUsuariosActivos.Text = _todosUsuarios.Count(u => u.Activo).ToString();
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = TxtBuscar.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(texto))
            {
                ListaUsuarios.ItemsSource = _todosUsuarios;
            }
            else
            {
                var filtrados = _todosUsuarios
                    .Where(u => u.Nombre.ToLower().Contains(texto) ||
                                u.UserName.ToLower().Contains(texto))
                    .ToList();
                ListaUsuarios.ItemsSource = filtrados;
            }
        }

        private void BtnNuevoUsuario_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Views.UsuarioDialog();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                var (nombre, username, contrasena, rol) = dialog.ObtenerDatos();

                var (ok, mensaje) = UsuarioController.Crear(nombre, username, contrasena, rol);

                if (ok)
                {
                    MessageBox.Show("✅ " + mensaje, "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarUsuarios();
                }
                else
                {
                    MessageBox.Show("⚠️ " + mensaje, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var usuarioId = (int)btn.Tag;
            var usuario = _todosUsuarios.FirstOrDefault(u => u.Id == usuarioId);

            if (usuario == null) return;

            var dialog = new Views.UsuarioDialog(usuario);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                var (nombre, username, _, rol) = dialog.ObtenerDatos();

                var (ok, mensaje) = UsuarioController.Actualizar(usuarioId, nombre, username, rol);

                if (ok)
                {
                    MessageBox.Show("✅ " + mensaje, "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarUsuarios();
                }
                else
                {
                    MessageBox.Show("⚠️ " + mensaje, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnCambiarContrasena_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var usuarioId = (int)btn.Tag;
            var usuario = _todosUsuarios.FirstOrDefault(u => u.Id == usuarioId);

            if (usuario == null) return;

            var dialog = new Views.CambiarContrasenaDialog(usuario.Nombre);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                var nuevaContrasena = dialog.ObtenerContrasena();

                var (ok, mensaje) = UsuarioController.CambiarContrasena(usuarioId, nuevaContrasena);

                if (ok)
                {
                    MessageBox.Show("✅ " + mensaje, "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("⚠️ " + mensaje, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var usuarioId = (int)btn.Tag;
            var usuario = _todosUsuarios.FirstOrDefault(u => u.Id == usuarioId);

            if (usuario == null) return;

            if (usuario.Id == _usuario.Id)
            {
                MessageBox.Show("⚠️ No puedes eliminar tu propio usuario.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmar = MessageBox.Show(
                $"¿Estás seguro de eliminar al usuario '{usuario.Nombre}'?\nEsta acción no se puede deshacer.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmar == MessageBoxResult.Yes)
            {
                var (ok, mensaje) = UsuarioController.Eliminar(usuarioId);

                if (ok)
                {
                    MessageBox.Show("✅ " + mensaje, "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarUsuarios();
                }
                else
                {
                    MessageBox.Show("⚠️ " + mensaje, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}
