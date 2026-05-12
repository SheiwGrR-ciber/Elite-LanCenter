// =============================================
// ELITE LAN CENTER - INVENTARIO PAGE
// =============================================

using EliteLanCenter.Controllers;
using EliteLanCenter.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EliteLanCenter.Pages
{
    public partial class InventarioPage : Page
    {
        private readonly Usuario _usuario;
        private List<Producto> _todosProductos = new();

        public InventarioPage(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            CargarProductos();
        }

        private void CargarProductos()
        {
            _todosProductos = ProductoController.ObtenerTodos();
            ActualizarLista(_todosProductos);
            ActualizarTotales();
        }

        private void ActualizarLista(List<Producto> productos)
        {
            foreach (var p in productos)
            {
                p.StockAlertaColor = p.StockMostrador <= 0
                    ? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44))
                    : p.StockMostrador <= 5
                        ? new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B))
                        : new SolidColorBrush(Color.FromRgb(0x01, 0xEF, 0xAC));

                p.StockAlertaTexto = p.StockMostrador <= 0
                    ? Brushes.White
                    : p.StockMostrador <= 5
                        ? Brushes.Black
                        : Brushes.White;
            }

            ListaProductos.ItemsSource = productos;
        }

        private void ActualizarTotales()
        {
            var count = _todosProductos.Count;
            var valorTotal = _todosProductos.Sum(p => p.ValorTotal);
            LblTotalProductos.Text = $"{count} productos | Valor total: S/ {valorTotal:F2}";
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = TxtBuscar.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(texto))
            {
                ActualizarLista(_todosProductos);
            }
            else
            {
                var filtrados = _todosProductos
                    .Where(p => p.Nombre.ToLower().Contains(texto))
                    .ToList();
                ActualizarLista(filtrados);
            }
        }

        private void BtnNuevoProducto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Views.ProductoDialog();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                var (nombre, precioUnidad, precioPaquete, unidadesPaquete) = dialog.ObtenerDatos();

                var (ok, mensaje) = ProductoController.Agregar(
                    nombre, precioUnidad, precioPaquete, unidadesPaquete);

                if (ok)
                {
                    MessageBox.Show("✅ " + mensaje, "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarProductos();
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
            var productoId = (int)btn.Tag;
            var producto = ProductoController.ObtenerPorId(productoId);

            if (producto == null) return;

            var dialog = new Views.ProductoDialog(producto);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                var (nombre, precioUnidad, precioPaquete, unidadesPaquete) = dialog.ObtenerDatos();

                var (ok, mensaje) = ProductoController.Actualizar(
                    productoId, nombre, precioUnidad, precioPaquete, unidadesPaquete);

                if (ok)
                {
                    MessageBox.Show("✅ " + mensaje, "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarProductos();
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
            var productoId = (int)btn.Tag;
            var producto = ProductoController.ObtenerPorId(productoId);

            if (producto == null) return;

            var confirmar = MessageBox.Show(
                $"¿Estás seguro de eliminar el producto '{producto.Nombre}'?\nEsta acción no se puede deshacer.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmar == MessageBoxResult.Yes)
            {
                var (ok, mensaje) = ProductoController.Desactivar(productoId);

                if (ok)
                {
                    MessageBox.Show("✅ " + mensaje, "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarProductos();
                }
                else
                {
                    MessageBox.Show("⚠️ " + mensaje, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnTrasladarProducto_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var productoId = (int)btn.Tag;
            var producto = ProductoController.ObtenerPorId(productoId);

            if (producto == null) return;

            var dialog = new Views.TrasladoStockDialog(producto);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                var (paquetes, unidades) = dialog.ObtenerDatos();

                if (paquetes > 0 || unidades > 0)
                {
                    var nuevoStockMostrador = producto.StockMostrador + unidades + (paquetes * producto.UnidadesPaquete);
                    var nuevoPaquetes = producto.PaquetesAlmacen - paquetes;
                    var nuevoUnidades = producto.UnidadesSueltas - unidades + (paquetes > 0 ? producto.UnidadesPaquete : 0);

                    if (nuevoUnidades < 0 && producto.PaquetesAlmacen >= paquetes)
                    {
                        nuevoUnidades = producto.UnidadesSueltas;
                    }

                    ProductoController.ActualizarStockMostrador(productoId, nuevoStockMostrador);
                    ProductoController.ActualizarStockAlmacen(productoId, Math.Max(0, producto.PaquetesAlmacen - paquetes), Math.Max(0, nuevoUnidades));

                    MessageBox.Show("✅ Traslado realizado correctamente.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarProductos();
                }
            }
        }

        private void BtnTrasladar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Selecciona un producto y usa el botón 📦 para trasladar stock a mostrador.",
                "Trasladar Stock", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarProductos();
            MessageBox.Show("✅ Lista actualizada.", "Inventario",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
