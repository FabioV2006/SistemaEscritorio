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
using System.Windows.Shapes;
using CapaDatos;
using CapaNegocio;

namespace CapaPresentación.Vistas
{
    public class DevolucionItemViewModel : INotifyPropertyChanged
    {
        public int IdDetalleVenta { get; set; }
        public int IdLote { get; set; }
        public string NombreProducto { get; set; }
        public string NumeroLote { get; set; }
        public int CantidadVendida { get; set; }
        public decimal PrecioUnitario { get; set; }

        private int _cantidadADevolver;
        public int CantidadADevolver
        {
            get { return _cantidadADevolver; }
            set
            {
                if (value > CantidadVendida)
                    _cantidadADevolver = CantidadVendida;
                else if (value < 0)
                    _cantidadADevolver = 0;
                else
                    _cantidadADevolver = value;

                OnPropertyChanged(nameof(CantidadADevolver));
                OnPropertyChanged(nameof(SubTotalDevolucion));
            }
        }

        public string Motivo { get; set; }

        public decimal SubTotalDevolucion
        {
            get { return CantidadADevolver * PrecioUnitario; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public partial class DevolucionClientePage : Page
    {
        private CN_Venta cn_venta = new CN_Venta();
        private CN_DevolucionCliente cn_devolucion = new CN_DevolucionCliente();

        private VENTAS ventaEncontrada;
        private USUARIOS _usuarioActual;

        private ObservableCollection<DevolucionItemViewModel> itemsParaDevolver;

        public DevolucionClientePage()
        {
            InitializeComponent();
            itemsParaDevolver = new ObservableCollection<DevolucionItemViewModel>();
            dgProductosVendidos.ItemsSource = itemsParaDevolver;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            _usuarioActual = mainWindow.UsuarioActual;
        }

        private void btnBuscarVenta_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LimpiarFormulario();

                // 1. Abrir el modal de búsqueda
                ModalBuscarVenta modal = new ModalBuscarVenta();
                if (modal.ShowDialog() != true)
                {
                    return; // Usuario canceló
                }

                // 2. Obtener la venta básica seleccionada
                VENTAS ventaResumen = modal.VentaSeleccionada;

                if (ventaResumen == null || string.IsNullOrEmpty(ventaResumen.NumeroDocumento))
                {
                    MessageBox.Show("No se seleccionó ninguna venta válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 3. Mostrar mensaje de carga
                txtNroDocVenta.Text = "Cargando...";
                this.Cursor = System.Windows.Input.Cursors.Wait;

                // 4. Obtener la venta completa con todos sus detalles
                ventaEncontrada = cn_venta.ObtenerVentaParaDevolucion(ventaResumen.NumeroDocumento);

                // 5. Validaciones exhaustivas
                if (ventaEncontrada == null)
                {
                    MessageBox.Show(
                        $"No se pudo cargar la venta con el número de documento: {ventaResumen.NumeroDocumento}\n\n" +
                        "Posibles causas:\n" +
                        "- La venta fue eliminada\n" +
                        "- Error de conexión con la base de datos",
                        "Error al Cargar Venta",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (ventaEncontrada.DETALLE_VENTAS == null || ventaEncontrada.DETALLE_VENTAS.Count == 0)
                {
                    MessageBox.Show(
                        $"La venta {ventaEncontrada.NumeroDocumento} no tiene productos asociados.\n\n" +
                        "Esto puede indicar un problema con los datos de la venta.",
                        "Venta Sin Detalles",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    LimpiarFormulario();
                    return;
                }

                if (ventaEncontrada.CLIENTES == null)
                {
                    MessageBox.Show(
                        $"No se pudo cargar la información del cliente de la venta {ventaEncontrada.NumeroDocumento}",
                        "Error de Datos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    LimpiarFormulario();
                    return;
                }

                // 6. Llenar el formulario
                txtNroDocVenta.Text = ventaEncontrada.NumeroDocumento;
                txtClienteVenta.Text = ventaEncontrada.CLIENTES.RazonSocial;
                txtFechaVenta.Text = ventaEncontrada.FechaRegistro?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

                // 7. Llenar la grilla con validaciones por cada detalle
                int detallesCargados = 0;
                foreach (var detalle in ventaEncontrada.DETALLE_VENTAS)
                {
                    // Validar que el detalle tenga lote
                    if (detalle.LOTES == null)
                    {
                        Console.WriteLine($"Advertencia: Detalle {detalle.IdDetalleVenta} sin lote asociado.");
                        continue;
                    }

                    // Validar que el lote tenga producto
                    if (detalle.LOTES.PRODUCTOS == null)
                    {
                        Console.WriteLine($"Advertencia: Lote {detalle.LOTES.IdLote} sin producto asociado.");
                        continue;
                    }

                    itemsParaDevolver.Add(new DevolucionItemViewModel
                    {
                        IdDetalleVenta = detalle.IdDetalleVenta,
                        IdLote = detalle.IdLote.Value,
                        NombreProducto = detalle.LOTES.PRODUCTOS.Nombre ?? "Producto sin nombre",
                        NumeroLote = detalle.LOTES.NumeroLote ?? "N/A",
                        CantidadVendida = detalle.Cantidad ?? 0,
                        PrecioUnitario = detalle.PrecioVentaUnitario ?? 0,
                        CantidadADevolver = 0,
                        Motivo = ""
                    });

                    detallesCargados++;
                }

                if (detallesCargados == 0)
                {
                    MessageBox.Show(
                        "No se pudieron cargar los productos de la venta.\n\n" +
                        "Verifique que la venta tenga productos con lotes válidos.",
                        "Sin Productos Válidos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    LimpiarFormulario();
                    return;
                }

                // 8. Suscribirse a los cambios de cada item
                foreach (var item in itemsParaDevolver)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }

                ActualizarTotalDevolucion();

                MessageBox.Show(
                    $"Venta cargada correctamente.\n" +
                    $"Productos encontrados: {detallesCargados}",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error inesperado al buscar la venta:\n\n{ex.Message}\n\n" +
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

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DevolucionItemViewModel.SubTotalDevolucion))
            {
                ActualizarTotalDevolucion();
            }
        }

        private void ActualizarTotalDevolucion()
        {
            decimal total = itemsParaDevolver.Sum(item => item.SubTotalDevolucion);
            txtMontoTotalDevolver.Text = $"Monto Total a Devolver: {total:C}";
        }

        private void btnRegistrarDevolucion_Click(object sender, RoutedEventArgs e)
        {
            if (ventaEncontrada == null)
            {
                MessageBox.Show("Debe buscar una venta válida primero.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            DataTable dtDetalle = new DataTable();
            dtDetalle.Columns.Add("IdLote", typeof(int));
            dtDetalle.Columns.Add("Cantidad", typeof(int));
            dtDetalle.Columns.Add("MontoUnitario", typeof(decimal));
            dtDetalle.Columns.Add("SubTotal", typeof(decimal));

            foreach (var item in itemsADevolver)
            {
                dtDetalle.Rows.Add(
                    item.IdLote,
                    item.CantidadADevolver,
                    item.PrecioUnitario,
                    item.SubTotalDevolucion
                );
            }

            DEVOLUCIONES_CLIENTES nuevaDevolucion = new DEVOLUCIONES_CLIENTES()
            {
                IdVenta = ventaEncontrada.IdVenta,
                IdUsuario = _usuarioActual.IdUsuario,
                Motivo = txtMotivoGeneral.Text,
                MontoTotalDevuelto = itemsADevolver.Sum(i => i.SubTotalDevolucion),
                FechaRegistro = DateTime.Now
            };

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
            ventaEncontrada = null;
            txtNroDocVenta.Text = "";
            txtClienteVenta.Text = "";
            txtFechaVenta.Text = "";
            txtMotivoGeneral.Text = "";
            itemsParaDevolver.Clear();
            ActualizarTotalDevolucion();
        }
    }
}