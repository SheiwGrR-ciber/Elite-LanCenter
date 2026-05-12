// =============================================
// ELITE LAN CENTER - NUEVO CASERO VIEW
// =============================================

using EliteLanCenter.Controllers;
using EliteLanCenter.Models;
using System.Windows;

namespace EliteLanCenter.Views
{
    public partial class NuevoCaseroView : Window
    {
        private readonly Usuario _usuario;

        public NuevoCaseroView(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            TxtNombre.Focus();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var nombre = TxtNombre.Text.Trim();
            var apodo = TxtApodo.Text.Trim();
            var telefono = TxtTelefono.Text.Trim();
            var rfid = TxtRFID.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                LblError.Text = "El nombre es obligatorio.";
                TxtNombre.Focus();
                return;
            }

            var (ok, mensaje) = CaseroController.Registrar(nombre, apodo, telefono, rfid);

            if (ok)
            {
                MessageBox.Show("✅ Casero creado correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                LblError.Text = $"⚠ {mensaje}";
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
