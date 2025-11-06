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
            try
            {
                LimpiarFormulario();

                // 1. Abrir el modal de búsqueda
                ModalBuscarCompra modal = new ModalBuscarCompra();
                if (modal.ShowDialog() != true)
                {
                    return; // Usuario canceló
                }

                // 2. Obtener la compra básica seleccionada
                COMPRAS compraResumen = modal.CompraSeleccionada;

                if (compraResumen == null || string.IsNullOrEmpty(compraResumen.NumeroDocumento))
                {
                    MessageBox.Show("No se seleccionó ninguna compra válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 3. Mostrar mensaje de carga
                txtNroDocCompra.Text = "Cargando...";
                this.Cursor = System.Windows.Input.Cursors.Wait;

                // 4. Obtener la compra completa con todos sus detalles
                compraEncontrada = cn_compra.ObtenerCompraParaDevolucion(compraResumen.NumeroDocumento);

                // 5. Validaciones exhaustivas
                if (compraEncontrada == null)
                {
                    MessageBox.Show(
                        $"No se pudo cargar la compra con el número de documento: {compraResumen.NumeroDocumento}\n\n" +
                        "Posibles causas:\n" +
                        "- La compra fue eliminada\n" +
                        "- Error de conexión con la base de datos",
                        "Error al Cargar Compra",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (compraEncontrada.DETALLE_COMPRAS == null || compraEncontrada.DETALLE_COMPRAS.Count == 0)
                {
                    MessageBox.Show(
                        $"La compra {compraEncontrada.NumeroDocumento} no tiene productos asociados.\n\n" +
                        "Esto puede indicar un problema con los datos de la compra.",
                        "Compra Sin Detalles",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    LimpiarFormulario();
                    return;
                }

                if (compraEncontrada.PROVEEDORES == null)
                {
                    MessageBox.Show(
                        $"No se pudo cargar la información del proveedor de la compra {compraEncontrada.NumeroDocumento}",
                        "Error de Datos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    LimpiarFormulario();
                    return;
                }

                // 6. Llenar el formulario
                txtNroDocCompra.Text = compraEncontrada.NumeroDocumento;
                txtProveedor.Text = compraEncontrada.PROVEEDORES.RazonSocial;
                txtFechaCompra.Text = compraEncontrada.FechaRegistro?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

                // 7. Llenar la grilla con validaciones por cada detalle
                int detallesCargados = 0;
                foreach (var detalle in compraEncontrada.DETALLE_COMPRAS)
                {
                    // Validar que el detalle tenga lote
                    if (detalle.LOTES == null)
                    {
                        Console.WriteLine($"Advertencia: Detalle {detalle.IdDetalleCompra} sin lote asociado.");
                        continue;
                    }

                    // Validar que el lote tenga producto
                    if (detalle.LOTES.PRODUCTOS == null)
                    {
                        Console.WriteLine($"Advertencia: Lote {detalle.LOTES.IdLote} sin producto asociado.");
                        continue;
                    }

                    itemsParaDevolver.Add(new DevolucionProveedorItemViewModel
                    {
                        IdLote = detalle.IdLote.Value,
                        NombreProducto = detalle.LOTES.PRODUCTOS.Nombre ?? "Producto sin nombre",
                        NumeroLote = detalle.LOTES.NumeroLote ?? "N/A",
                        CantidadComprada = detalle.Cantidad ?? 0,
                        StockActualLote = detalle.LOTES.Stock, // El stock actual real
                        PrecioUnitarioCosto = detalle.PrecioCompraUnitario ?? 0,
                        CantidadADevolver = 0, // Inicia en 0
                        Motivo = ""
                    });

                    detallesCargados++;
                }

                if (detallesCargados == 0)
                {
                    MessageBox.Show(
                        "No se pudieron cargar los productos de la compra.\n\n" +
                        "Verifique que la compra tenga productos con lotes válidos.",
                        "Sin Productos Válidos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    LimpiarFormulario();
                    return;
                }

                // 8. Suscribirse a los eventos de cambio
                foreach (var item in itemsParaDevolver)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }

                ActualizarTotalDevolucion();

                MessageBox.Show(
                    $"Compra cargada correctamente.\n" +
                    $"Productos encontrados: {detallesCargados}",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error inesperado al buscar la compra:\n\n{ex.Message}\n\n" +
                    $"Detalles técnicos:\n{ex.StackTrace}",
                    "Error Crítico",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                LimpiarFormulario();
            }
            finally
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
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