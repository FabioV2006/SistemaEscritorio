using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CapaDatos;
using CapaNegocio;

namespace CapaPresentación.Vistas
{
    // --- ViewModel interno para la grilla editable ---
    public class DevolucionProveedorItemViewModel : INotifyPropertyChanged
    {
        // Propiedades de la compra original (ReadOnly)
        public int IdLote { get; set; }
        public string NombreProducto { get; set; }
        public string NumeroLote { get; set; }
        public int CantidadComprada { get; set; }
        public int StockActualLote { get; set; } // El stock actual de ese lote
        public decimal PrecioUnitarioCosto { get; set; } // El precio al que se compró

        // Propiedades editables por el usuario
        private int _cantidadADevolver;
        public int CantidadADevolver
        {
            get { return _cantidadADevolver; }
            set
            {
                // Validación: No se puede devolver más de lo que hay en stock
                if (value > StockActualLote)
                    _cantidadADevolver = StockActualLote;
                else if (value < 0)
                    _cantidadADevolver = 0;
                else
                    _cantidadADevolver = value;

                OnPropertyChanged(nameof(CantidadADevolver));
                OnPropertyChanged(nameof(SubTotalDevolucion)); // Avisar que el subtotal cambió
            }
        }

        public string Motivo { get; set; }

        // Propiedad calculada
        public decimal SubTotalDevolucion
        {
            get { return CantidadADevolver * PrecioUnitarioCosto; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    // --- Fin del ViewModel ---


    public partial class DevolucionProveedorPage : Page
    {
        private CN_Compra cn_compra = new CN_Compra();
        private CN_DevolucionProveedor cn_devolucion = new CN_DevolucionProveedor();

        private COMPRAS compraEncontrada;
        private USUARIOS _usuarioActual;

        private ObservableCollection<DevolucionProveedorItemViewModel> itemsParaDevolver;

        public DevolucionProveedorPage()
        {
            InitializeComponent();
            itemsParaDevolver = new ObservableCollection<DevolucionProveedorItemViewModel>();
            dgLotesComprados.ItemsSource = itemsParaDevolver;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            _usuarioActual = mainWindow.UsuarioActual;
        }

        private void btnBuscarCompra_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();

            // 1. Abrir el modal de búsqueda de Compras
            ModalBuscarCompra modal = new ModalBuscarCompra();
            if (modal.ShowDialog() == true)
            {
                // 2. Obtener la compra básica
                COMPRAS compraResumen = modal.CompraSeleccionada;

                // 3. Obtener la compra COMPLETA (con detalles y lotes)
                compraEncontrada = cn_compra.ObtenerCompraParaDevolucion(compraResumen.NumeroDocumento);

                if (compraEncontrada == null)
                {
                    MessageBox.Show("Error al cargar los detalles completos de la compra.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 4. Llenar el formulario
                txtNroDocCompra.Text = compraEncontrada.NumeroDocumento;
                txtProveedor.Text = compraEncontrada.PROVEEDORES.RazonSocial;
                txtFechaCompra.Text = compraEncontrada.FechaRegistro?.ToString("dd/MM/yyyy");

                // 5. Llenar la grilla con los lotes de esa compra
                foreach (var detalle in compraEncontrada.DETALLE_COMPRAS)
                {
                    itemsParaDevolver.Add(new DevolucionProveedorItemViewModel
                    {
                        IdLote = detalle.IdLote.Value,
                        NombreProducto = detalle.LOTES.PRODUCTOS.Nombre,
                        NumeroLote = detalle.LOTES.NumeroLote,
                        CantidadComprada = detalle.Cantidad.Value,
                        StockActualLote = detalle.LOTES.Stock, // El stock actual real
                        PrecioUnitarioCosto = detalle.PrecioCompraUnitario.Value,
                        CantidadADevolver = 0, // Inicia en 0
                        Motivo = ""
                    });
                }

                // Suscribirse a los eventos de cambio
                foreach (var item in itemsParaDevolver)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }

                ActualizarTotalDevolucion();
            }
        }

        // Se dispara cuando el usuario cambia la "Cantidad a Devolver"
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DevolucionProveedorItemViewModel.SubTotalDevolucion))
            {
                ActualizarTotalDevolucion();
            }
        }

        private void ActualizarTotalDevolucion()
        {
            decimal total = itemsParaDevolver.Sum(item => item.SubTotalDevolucion);
            txtMontoTotalDevolver.Text = $"Monto Total a Devolver (al costo): {total:C}";
        }

        private void btnRegistrarDevolucion_Click(object sender, RoutedEventArgs e)
        {
            if (compraEncontrada == null)
            {
                MessageBox.Show("Debe buscar una compra válida primero.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var itemsADevolver = itemsParaDevolver.Where(i => i.CantidadADevolver > 0).ToList();

            if (itemsADevolver.Count == 0)
            {
                MessageBox.Show("Debe ingresar una cantidad a devolver en al menos un producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtMotivoGeneral.Text))
            {
                MessageBox.Show("Debe ingresar un motivo general para la devolución.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Crear el DataTable para la CapaNegocio
            DataTable dtDetalle = new DataTable();
            dtDetalle.Columns.Add("IdLote", typeof(int));
            dtDetalle.Columns.Add("Cantidad", typeof(int));
            dtDetalle.Columns.Add("MontoUnitario", typeof(decimal));
            dtDetalle.Columns.Add("SubTotal", typeof(decimal));

            foreach (var item in itemsADevolver)
            {
                // VALIDACIÓN DE ÚLTIMO MINUTO: Asegurarse de que la cantidad a devolver
                // no sea mayor que el stock actual (en caso de que la UI no se refresque)
                if (item.CantidadADevolver > item.StockActualLote)
                {
                    MessageBox.Show($"Error de Stock en Lote {item.NumeroLote}. Stock actual: {item.StockActualLote}, usted intenta devolver: {item.CantidadADevolver}", "Error de Stock", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                dtDetalle.Rows.Add(
                    item.IdLote,
                    item.CantidadADevolver,
                    item.PrecioUnitarioCosto, // Usamos el precio de costo
                    item.SubTotalDevolucion
                );
            }

            // 3. Crear el objeto Devolucion
            DEVOLUCIONES_PROVEEDORES nuevaDevolucion = new DEVOLUCIONES_PROVEEDORES()
            {
                IdCompra = compraEncontrada.IdCompra,
                IdUsuario = _usuarioActual.IdUsuario,
                Motivo = txtMotivoGeneral.Text,
                MontoTotalDevuelto = itemsADevolver.Sum(i => i.SubTotalDevolucion),
                FechaRegistro = DateTime.Now
            };

            // 4. Llamar a la CapaNegocio
            string mensaje = string.Empty;
            int idDevGenerada = cn_devolucion.Registrar(nuevaDevolucion, dtDetalle, out mensaje);

            if (idDevGenerada > 0)
            {
                MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarFormulario();
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimpiarFormulario()
        {
            compraEncontrada = null;
            txtNroDocCompra.Text = "";
            txtProveedor.Text = "";
            txtFechaCompra.Text = "";
            txtMotivoGeneral.Text = "";
            itemsParaDevolver.Clear();
            ActualizarTotalDevolucion();
        }
    }
}