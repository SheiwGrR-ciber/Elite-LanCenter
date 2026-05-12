// =============================================
// ELITE LAN CENTER - PRODUCTO DIALOG
// =============================================

using EliteLanCenter.Models;
using System.Windows;

namespace EliteLanCenter.Views
{
    public partial class ProductoDialog : Window
    {
        private readonly int? _productoId;

        public ProductoDialog()
        {
            InitializeComponent();
            _productoId = null;
            LblTitulo.Text = "Nuevo Producto";
            TxtNombre.Focus();
        }

        public ProductoDialog(Producto producto) : this()
        {
            _productoId = producto.Id;
            LblTitulo.Text = "Editar Producto";
            TxtNombre.Text = producto.Nombre;
            TxtPrecioUnidad.Text = producto.PrecioUnidad.ToString("F2");
            TxtPrecioPaquete.Text = producto.PrecioPaquete.ToString("F2");
            TxtUnidadesPaquete.Text = producto.UnidadesPaquete.ToString();
        }

        public (string nombre, double precioUnidad, double precioPaquete, int unidadesPaquete) ObtenerDatos()
        {
            double.TryParse(TxtPrecioUnidad.Text.Replace(",", "."),
                out double precioUnidad);
            double.TryParse(TxtPrecioPaquete.Text.Replace(",", "."),
                out double precioPaquete);
            int.TryParse(TxtUnidadesPaquete.Text, out int unidadesPaquete);

            return (TxtNombre.Text.Trim(), precioUnidad, precioPaquete, unidadesPaquete);
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var nombre = TxtNombre.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                LblError.Text = "El nombre es obligatorio.";
                TxtNombre.Focus();
                return;
            }

            if (!double.TryParse(TxtPrecioUnidad.Text.Replace(",", "."), out double precioUnidad) || precioUnidad <= 0)
            {
                LblError.Text = "Ingresa un precio unitario válido mayor a 0.";
                TxtPrecioUnidad.Focus();
                return;
            }

            if (!int.TryParse(TxtUnidadesPaquete.Text, out int unidadesPaquete) || unidadesPaquete <= 0)
            {
                LblError.Text = "Ingresa un número de unidades válido mayor a 0.";
                TxtUnidadesPaquete.Focus();
                return;
            }

            double.TryParse(TxtPrecioPaquete.Text.Replace(",", "."), out double precioPaquete);

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
