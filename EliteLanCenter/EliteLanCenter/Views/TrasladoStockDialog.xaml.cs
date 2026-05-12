// =============================================
// ELITE LAN CENTER - TRASLADO STOCK DIALOG
// =============================================

using EliteLanCenter.Models;
using System.Windows;

namespace EliteLanCenter.Views
{
    public partial class TrasladoStockDialog : Window
    {
        private readonly Producto _producto;

        public TrasladoStockDialog(Producto producto)
        {
            InitializeComponent();
            _producto = producto;

            LblProducto.Text = $"Producto: {_producto.Nombre}";
            LblStockActual.Text = $"Stock actual - Mostrador: {_producto.StockMostrador} | Almacén: {_producto.PaquetesAlmacen} pkg + {_producto.UnidadesSueltas} und";

            TxtPaquetes.Focus();
        }

        public (int paquetes, int unidades) ObtenerDatos()
        {
            int.TryParse(TxtPaquetes.Text, out int paquetes);
            int.TryParse(TxtUnidades.Text, out int unidades);
            return (paquetes, unidades);
        }

        private void BtnTrasladar_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtPaquetes.Text, out int paquetes))
            {
                LblError.Text = "Ingresa un número válido de paquetes.";
                TxtPaquetes.Focus();
                return;
            }

            if (!int.TryParse(TxtUnidades.Text, out int unidades))
            {
                LblError.Text = "Ingresa un número válido de unidades.";
                TxtUnidades.Focus();
                return;
            }

            if (paquetes <= 0 && unidades <= 0)
            {
                LblError.Text = "Debes trasladar al menos 1 paquete o 1 unidad.";
                return;
            }

            if (paquetes > _producto.PaquetesAlmacen)
            {
                LblError.Text = $"Solo tienes {_producto.PaquetesAlmacen} paquetes en almacén.";
                return;
            }

            var totalUnidades = unidades + (paquetes * _producto.UnidadesPaquete);
            var disponibles = (_producto.PaquetesAlmacen - paquetes) * _producto.UnidadesPaquete + _producto.UnidadesSueltas;

            if (unidades > disponibles && paquetes <= 0)
            {
                LblError.Text = $"Solo tienes {_producto.UnidadesSueltas} unidades sueltas disponibles.";
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
