using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CapaDatos;
using CapaNegocio;

namespace CapaPresentación.Vistas
{
    // Clase interna para manejar la vista de la grilla (ViewModel)
    public class DetalleCompraViewModel
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string NumeroLote { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Cantidad { get; set; }
        public decimal MontoTotal { get; set; }
    }


    public partial class RegistrarCompraPage : Page
    {
        private CN_Compra cn_compra = new CN_Compra();
        private List<DetalleCompraViewModel> detalleCompra = new List<DetalleCompraViewModel>();
        private PRODUCTOS productoSeleccionado; // El producto seleccionado del modal
        private PROVEEDORES proveedorSeleccionado; // El proveedor seleccionado
        private USUARIOS _usuarioActual;

        public RegistrarCompraPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Obtenemos el usuario actual desde la MainWindow
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            _usuarioActual = mainWindow.UsuarioActual; // (Necesitarás crear esta propiedad pública en MainWindow)

            // Simulación si no se puede obtener (BORRAR DESPUÉS)
            if (_usuarioActual == null)
                _usuarioActual = new USUARIOS() { IdUsuario = 1 };
        }

        private void btnBuscarProveedor_Click(object sender, RoutedEventArgs e)
        {
            ModalBuscarProveedor modal = new ModalBuscarProveedor();
            if (modal.ShowDialog() == true)
            {
                proveedorSeleccionado = modal.ProveedorSeleccionado;
                txtIdProveedor.Text = proveedorSeleccionado.IdProveedor.ToString();
                txtRazonSocialProv.Text = proveedorSeleccionado.RazonSocial;
            }
        }

        private void btnBuscarProducto_Click(object sender, RoutedEventArgs e)
        {
            ModalBuscarProducto modal = new ModalBuscarProducto();
            if (modal.ShowDialog() == true)
            {
                productoSeleccionado = modal.ProductoSeleccionado;
                txtIdProducto.Text = productoSeleccionado.IdProducto.ToString();
                txtProductoNombre.Text = productoSeleccionado.Nombre;
            }
        }

        private void btnAnadir_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones
            if (productoSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(txtLoteNumero.Text))
            {
                MessageBox.Show("Debe ingresar un número de lote.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!dpFechaVenc.SelectedDate.HasValue)
            {
                MessageBox.Show("Debe seleccionar una fecha de vencimiento.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!decimal.TryParse(txtPrecioCompra.Text, out decimal precioCompra) || precioCompra <= 0)
            {
                MessageBox.Show("Precio de compra no válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!decimal.TryParse(txtPrecioVenta.Text, out decimal precioVenta) || precioVenta <= 0)
            {
                MessageBox.Show("Precio de venta no válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Cantidad no válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Crear el item para la grilla
            var item = new DetalleCompraViewModel
            {
                IdProducto = productoSeleccionado.IdProducto,
                NombreProducto = productoSeleccionado.Nombre,
                NumeroLote = txtLoteNumero.Text,
                FechaVencimiento = dpFechaVenc.SelectedDate.Value,
                PrecioCompra = precioCompra,
                PrecioVenta = precioVenta,
                Cantidad = cantidad,
                MontoTotal = precioCompra * cantidad
            };

            detalleCompra.Add(item);

            // Refrescar la grilla
            ActualizarDataGridYTotal();

            // Limpiar campos
            LimpiarCamposProducto();
        }

        private void btnEliminarItem_Click(object sender, RoutedEventArgs e)
        {
            DetalleCompraViewModel itemSeleccionado = (sender as Button).DataContext as DetalleCompraViewModel;
            if (itemSeleccionado != null)
            {
                detalleCompra.Remove(itemSeleccionado);
                ActualizarDataGridYTotal();
            }
        }

        private void btnRegistrarCompra_Click(object sender, RoutedEventArgs e)
        {
            if (proveedorSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un proveedor.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (detalleCompra.Count == 0)
            {
                MessageBox.Show("Debe añadir al menos un producto a la compra.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Crear el DataTable para la CapaNegocio
            DataTable dtDetalle = new DataTable();
            dtDetalle.Columns.Add("IdProducto", typeof(int));
            dtDetalle.Columns.Add("NumeroLote", typeof(string));
            dtDetalle.Columns.Add("FechaVencimiento", typeof(DateTime));
            dtDetalle.Columns.Add("PrecioCompra", typeof(decimal));
            dtDetalle.Columns.Add("PrecioVenta", typeof(decimal));
            dtDetalle.Columns.Add("Cantidad", typeof(int));
            dtDetalle.Columns.Add("MontoTotal", typeof(decimal));

            foreach (var item in detalleCompra)
            {
                dtDetalle.Rows.Add(
                    item.IdProducto,
                    item.NumeroLote,
                    item.FechaVencimiento,
                    item.PrecioCompra,
                    item.PrecioVenta,
                    item.Cantidad,
                    item.MontoTotal
                );
            }

            // 2. Crear el objeto Compra
            COMPRAS nuevaCompra = new COMPRAS()
            {
                IdUsuario = _usuarioActual.IdUsuario,
                IdProveedor = proveedorSeleccionado.IdProveedor,
                TipoDocumento = (cboTipoDocumento.SelectedItem as ComboBoxItem).Content.ToString(),
                NumeroDocumento = txtNumeroDocumento.Text,
                MontoTotal = detalleCompra.Sum(d => d.MontoTotal),
                FechaRegistro = DateTime.Now
            };

            // 3. Llamar a la CapaNegocio
            string mensaje = string.Empty;
            int idCompraGenerada = cn_compra.Registrar(nuevaCompra, dtDetalle, out mensaje);

            if (idCompraGenerada > 0)
            {
                MessageBox.Show(mensaje + $"\nNro de Compra: {idCompraGenerada}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                // Aquí iría el flujo de "Imprimir / Nuevo" que hicimos
                LimpiarFormularioCompleto();
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- MÉTODOS DE AYUDA ---

        private void ActualizarDataGridYTotal()
        {
            dgDetalleCompra.ItemsSource = null;
            dgDetalleCompra.ItemsSource = detalleCompra;

            decimal montoTotal = detalleCompra.Sum(d => d.MontoTotal);
            txtMontoTotal.Text = $"Monto Total Compra: {montoTotal:C}"; // "C" = Formato de Moneda
        }

        private void LimpiarCamposProducto()
        {
            productoSeleccionado = null;
            txtIdProducto.Text = "";
            txtProductoNombre.Text = "";
            txtLoteNumero.Text = "";
            dpFechaVenc.SelectedDate = null;
            txtPrecioCompra.Text = "";
            txtPrecioVenta.Text = "";
            txtCantidad.Text = "";
        }

        private void LimpiarFormularioCompleto()
        {
            proveedorSeleccionado = null;
            txtIdProveedor.Text = "";
            txtRazonSocialProv.Text = "";
            cboTipoDocumento.SelectedIndex = 0;
            txtNumeroDocumento.Text = "";

            LimpiarCamposProducto();

            detalleCompra.Clear();
            ActualizarDataGridYTotal();
        }

        // Validador para que los TextBox solo acepten números
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Permite números y un solo punto decimal
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text) || (e.Text == "." && ((TextBox)sender).Text.Contains("."));
        }
    }
}