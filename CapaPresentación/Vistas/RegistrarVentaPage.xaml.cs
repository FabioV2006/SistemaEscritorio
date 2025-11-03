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
    // Clase interna (ViewModel) para manejar la vista de la grilla
    // --- CORRECCIÓN DE SYNTAX AQUÍ ---
    public class DetalleVentaViewModel
    {
        public int IdLote { get; set; } // Corregido: de 'get.' a 'get;'
        public string NombreProducto { get; set; }
        public string NumeroLote { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal PrecioVentaUnitario { get; set; }
        public int Cantidad { get; set; }
        public int StockDisponible { get; set; } // Guardamos el stock original
        public decimal SubTotal { get; set; }
    }


    public partial class RegistrarVentaPage : Page
    {
        private CN_Venta cn_venta = new CN_Venta();
        private CN_Lote cn_lote = new CN_Lote(); // <--- Necesario para buscar lotes

        private List<DetalleVentaViewModel> detalleVenta = new List<DetalleVentaViewModel>();
        private PRODUCTOS productoSeleccionado; // El producto seleccionado
        private CLIENTES clienteSeleccionado; // El cliente seleccionado
        private USUARIOS _usuarioActual;

        public RegistrarVentaPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Obtenemos el usuario actual desde la MainWindow
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            _usuarioActual = mainWindow.UsuarioActual;
        }

        private void btnBuscarCliente_Click(object sender, RoutedEventArgs e)
        {
            ModalBuscarCliente modal = new ModalBuscarCliente();
            if (modal.ShowDialog() == true)
            {
                clienteSeleccionado = modal.ClienteSeleccionado;
                txtIdCliente.Text = clienteSeleccionado.IdCliente.ToString();
                txtRazonSocialCliente.Text = clienteSeleccionado.RazonSocial;
            }
        }

        // --- LÓGICA DE BÚSQUEDA CORREGIDA ---
        private void btnBuscarProducto_Click(object sender, RoutedEventArgs e)
        {
            // 1. Abrimos el modal para buscar un PRODUCTO
            //    (Asegúrate de tener ModalBuscarProducto.xaml y .cs en tu proyecto)
            ModalBuscarProducto modal = new ModalBuscarProducto();
            if (modal.ShowDialog() == true)
            {
                // 2. Guardamos el producto y limpiamos campos
                productoSeleccionado = modal.ProductoSeleccionado;
                txtIdProducto.Text = productoSeleccionado.IdProducto.ToString();
                txtProductoNombre.Text = productoSeleccionado.Nombre;
                LimpiarCamposLote(); // Limpia los campos de texto del lote

                // 3. (NUEVO) Buscamos en la BD todos los lotes para ESE producto
                List<LOTES> lotesDisponibles = cn_lote.ListarLotesPorProducto(productoSeleccionado.IdProducto);

                if (lotesDisponibles.Count == 0)
                {
                    MessageBox.Show("No hay lotes con stock para este producto.", "Stock Cero", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cboLotesDisponibles.ItemsSource = null;
                    return;
                }

                // 4. Llenamos el ComboBox de Lotes
                cboLotesDisponibles.ItemsSource = lotesDisponibles;
                // Usamos un truco para mostrar Lote + Stock + Vencimiento en el ComboBox
                cboLotesDisponibles.DisplayMemberPath = "InfoParaVenta"; // <-- Necesitas añadir esta propiedad a tu clase LOTES.cs
                cboLotesDisponibles.SelectedValuePath = "IdLote";
                cboLotesDisponibles.SelectedIndex = 0; // Seleccionamos el primero (el que vence antes)
            }
        }

        // Evento que se dispara cuando el usuario selecciona un lote del ComboBox
        private void cboLotesDisponibles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboLotesDisponibles.SelectedItem is LOTES loteSeleccionado)
            {
                // Obtenemos el stock REAL disponible (considerando lo que ya está en el carrito)
                int stockEnCarrito = detalleVenta
                    .Where(l => l.IdLote == loteSeleccionado.IdLote)
                    .Sum(l => l.Cantidad);

                int stockRealDisponible = loteSeleccionado.Stock - stockEnCarrito;

                // Rellenamos los campos con la info del lote
                txtLoteStock.Text = stockRealDisponible.ToString();
                txtLoteVenc.Text = loteSeleccionado.FechaVencimiento.ToString("dd/MM/yyyy");
                txtPrecioVenta.Text = loteSeleccionado.PrecioVenta.ToString("F2");
                txtCantidad.Text = "1";
            }
        }

        private void btnAnadir_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validaciones
            if (cboLotesDisponibles.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un producto y un lote.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Cantidad no válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!decimal.TryParse(txtPrecioVenta.Text, out decimal precioVenta) || precioVenta <= 0)
            {
                MessageBox.Show("Precio de venta no válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LOTES loteSeleccionado = (LOTES)cboLotesDisponibles.SelectedItem;

            // 2. Validación de Stock
            int stockDisponible;
            int.TryParse(txtLoteStock.Text, out stockDisponible); // Leemos el stock YA CALCULADO

            if (cantidad > stockDisponible)
            {
                MessageBox.Show($"Stock insuficiente. Solo quedan {stockDisponible} unidades de este lote.", "Error de Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Si el item ya está en el carrito, solo actualizamos la cantidad
            var itemExistenteEnCarrito = detalleVenta.FirstOrDefault(l => l.IdLote == loteSeleccionado.IdLote);

            if (itemExistenteEnCarrito != null)
            {
                itemExistenteEnCarrito.Cantidad += cantidad;
                itemExistenteEnCarrito.SubTotal = itemExistenteEnCarrito.Cantidad * itemExistenteEnCarrito.PrecioVentaUnitario;
            }
            else // 4. Si es nuevo, lo añadimos
            {
                var item = new DetalleVentaViewModel
                {
                    IdLote = loteSeleccionado.IdLote,
                    NombreProducto = productoSeleccionado.Nombre, // Usamos el producto ya guardado
                    NumeroLote = loteSeleccionado.NumeroLote,
                    FechaVencimiento = loteSeleccionado.FechaVencimiento,
                    PrecioVentaUnitario = precioVenta,
                    Cantidad = cantidad,
                    StockDisponible = loteSeleccionado.Stock, // Guardamos el stock original de la BD
                    SubTotal = precioVenta * cantidad
                };
                detalleVenta.Add(item);
            }

            ActualizarDataGridYTotal();
            LimpiarCamposProducto();
        }

        private void btnEliminarItem_Click(object sender, RoutedEventArgs e)
        {
            DetalleVentaViewModel itemSeleccionado = (sender as Button).DataContext as DetalleVentaViewModel;
            if (itemSeleccionado != null)
            {
                detalleVenta.Remove(itemSeleccionado);
                ActualizarDataGridYTotal();
            }
        }

        private void btnRegistrarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (clienteSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (detalleVenta.Count == 0)
            {
                MessageBox.Show("Debe añadir al menos un producto a la venta.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal.TryParse(txtMontoPago.Text, out decimal montoPago);
            decimal montoTotal = detalleVenta.Sum(d => d.SubTotal);
            decimal montoCambio = montoPago - montoTotal;
            if (montoCambio < 0) montoCambio = 0;

            // 1. Crear el DataTable para la CapaNegocio
            DataTable dtDetalle = new DataTable();
            dtDetalle.Columns.Add("IdLote", typeof(int));
            dtDetalle.Columns.Add("Cantidad", typeof(int));
            dtDetalle.Columns.Add("PrecioVentaUnitario", typeof(decimal));
            dtDetalle.Columns.Add("SubTotal", typeof(decimal));

            foreach (var item in detalleVenta)
            {
                dtDetalle.Rows.Add(item.IdLote, item.Cantidad, item.PrecioVentaUnitario, item.SubTotal);
            }

            // 2. Crear el objeto Venta
            VENTAS nuevaVenta = new VENTAS()
            {
                IdUsuario = _usuarioActual.IdUsuario,
                IdCliente = clienteSeleccionado.IdCliente,
                TipoDocumento = (cboTipoDocumento.SelectedItem as ComboBoxItem).Content.ToString(),
                NumeroDocumento = txtNumeroDocumento.Text,
                MontoPago = montoPago,
                MontoCambio = montoCambio,
                MontoTotal = montoTotal,
                FechaRegistro = DateTime.Now
            };

            // 3. Llamar a la CapaNegocio
            string mensaje = string.Empty;
            int idVentaGenerada = cn_venta.Registrar(nuevaVenta, dtDetalle, out mensaje);

            if (idVentaGenerada > 0)
            {
                MessageBox.Show(mensaje + $"\nNro de Venta: {idVentaGenerada}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
            dgDetalleVenta.ItemsSource = null;
            dgDetalleVenta.ItemsSource = detalleVenta;

            decimal montoTotal = detalleVenta.Sum(d => d.SubTotal);
            txtMontoTotal.Text = $"Monto Total: {montoTotal:C}";

            txtMontoPago_LostFocus(null, null);
        }

        private void LimpiarCamposProducto()
        {
            productoSeleccionado = null;
            txtIdProducto.Text = "";
            txtProductoNombre.Text = "";
            cboLotesDisponibles.ItemsSource = null;
            LimpiarCamposLote();
        }

        private void LimpiarCamposLote()
        {
            txtLoteStock.Text = "";
            txtLoteVenc.Text = "";
            txtPrecioVenta.Text = "";
            txtCantidad.Text = "";
        }

        private void LimpiarFormularioCompleto()
        {
            clienteSeleccionado = null;
            txtIdCliente.Text = "";
            txtRazonSocialCliente.Text = "";
            cboTipoDocumento.SelectedIndex = 0;
            txtNumeroDocumento.Text = "";
            txtMontoPago.Text = "";
            txtMontoCambio.Text = "";

            LimpiarCamposProducto();

            detalleVenta.Clear();
            ActualizarDataGridYTotal();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text) || (e.Text == "." && ((TextBox)sender).Text.Contains("."));
        }

        private void txtMontoPago_LostFocus(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtMontoPago.Text, out decimal montoPago) && montoPago > 0)
            {
                decimal montoTotal = detalleVenta.Sum(d => d.SubTotal);
                decimal montoCambio = montoPago - montoTotal;
                txtMontoCambio.Text = (montoCambio < 0) ? "S/ 0.00" : montoCambio.ToString("C");
            }
            else
            {
                txtMontoCambio.Text = "S/ 0.00";
            }
        }
    }
}