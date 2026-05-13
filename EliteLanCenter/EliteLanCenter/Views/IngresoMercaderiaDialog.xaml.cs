using EliteLanCenter.Models;
using System.Windows;

namespace EliteLanCenter.Views
{
    public partial class IngresoMercaderiaDialog : Window
    {
        public Producto? ProductoSeleccionado => CmbProducto.SelectedItem as Producto;

        public IngresoMercaderiaDialog(List<Producto> productos)
        {
            InitializeComponent();
            CmbProducto.ItemsSource = productos;
            if (productos.Count > 0)
                CmbProducto.SelectedIndex = 0;
            TxtPaquetes.Focus();
        }

        public (int paquetes, int unidades, double costoTotal) ObtenerDatos()
        {
            int.TryParse(TxtPaquetes.Text, out int paquetes);
            int.TryParse(TxtUnidades.Text, out int unidades);
            double.TryParse(TxtCostoTotal.Text.Replace(",", "."), out double costo);
            return (paquetes, unidades, costo);
        }

        private void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            if (CmbProducto.SelectedItem == null)
            {
                LblError.Text = "Selecciona un producto.";
                CmbProducto.Focus();
                return;
            }

            int.TryParse(TxtPaquetes.Text, out int paquetes);
            int.TryParse(TxtUnidades.Text, out int unidades);
            double.TryParse(TxtCostoTotal.Text.Replace(",", "."), out double costo);

            if (paquetes < 0 || unidades < 0)
            {
                LblError.Text = "Las cantidades no pueden ser negativas.";
                return;
            }

            if (paquetes == 0 && unidades == 0)
            {
                LblError.Text = "Debes ingresar al menos 1 paquete o unidad suelta.";
                TxtPaquetes.Focus();
                return;
            }

            if (costo <= 0)
            {
                LblError.Text = "Ingresa el costo total de la mercadería.";
                TxtCostoTotal.Focus();
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
