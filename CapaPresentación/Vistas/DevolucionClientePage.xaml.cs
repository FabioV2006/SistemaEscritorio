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
    // --- ViewModel interno para la grilla editable ---
    // Implementa INotifyPropertyChanged para que el total se actualice en vivo
    public class DevolucionItemViewModel : INotifyPropertyChanged
    {
        // Propiedades de la venta original (ReadOnly)
        public int IdDetalleVenta { get; set; }
        public int IdLote { get; set; }
        public string NombreProducto { get; set; }
        public string NumeroLote { get; set; }
        public int CantidadVendida { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Propiedades editables por el usuario
        private int _cantidadADevolver;
        public int CantidadADevolver
        {
            get { return _cantidadADevolver; }
            set
            {
                if (value > CantidadVendida) // Validación
                    _cantidadADevolver = CantidadVendida;
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
            get { return CantidadADevolver * PrecioUnitario; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    // --- Fin del ViewModel ---


    public partial class DevolucionClientePage : Page
    {
        private CN_Venta cn_venta = new CN_Venta();
        private CN_DevolucionCliente cn_devolucion = new CN_DevolucionCliente();

        private VENTAS ventaEncontrada; // Almacena la venta COMPLETA (con detalles)
        private USUARIOS _usuarioActual;

        // Esta es la lista que se bindea a la grilla
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

        // --- ESTE ES EL NUEVO FLUJO DE BÚSQUEDA ---
        private void btnBuscarVenta_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();

            // 1. ABRIR EL MODAL DE BÚSQUEDA
            ModalBuscarVenta modal = new ModalBuscarVenta();
            if (modal.ShowDialog() == true)
            {
                // 2. OBTENER LA VENTA BÁSICA SELECCIONADA (SOLO TIENE Id, NroDoc, Cliente)
                VENTAS ventaResumen = modal.VentaSeleccionada;

                // 3. USAR EL NRO. DE DOCUMENTO PARA OBTENER LA VENTA COMPLETA
                //    (Esto es crucial, ya que carga los DETALLE_VENTAS, LOTES y PRODUCTOS)
                ventaEncontrada = cn_venta.ObtenerVentaParaDevolucion(ventaResumen.NumeroDocumento);

                if (ventaEncontrada == null)
                {
                    MessageBox.Show("Error al cargar los detalles completos de la venta seleccionada.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 4. LLENAR EL FORMULARIO (ahora sí se encontrará)
                txtNroDocVenta.Text = ventaEncontrada.NumeroDocumento; // Rellenamos el campo
                txtClienteVenta.Text = ventaEncontrada.CLIENTES.RazonSocial;
                txtFechaVenta.Text = ventaEncontrada.FechaRegistro?.ToString("dd/MM/yyyy");

                // 5. Llenar la grilla con los productos/lotes de esa venta
                foreach (var detalle in ventaEncontrada.DETALLE_VENTAS)
                {
                    itemsParaDevolver.Add(new DevolucionItemViewModel
                    {
                        IdDetalleVenta = detalle.IdDetalleVenta,
                        IdLote = detalle.IdLote.Value,
                        NombreProducto = detalle.LOTES.PRODUCTOS.Nombre,
                        NumeroLote = detalle.LOTES.NumeroLote,
                        CantidadVendida = detalle.Cantidad.Value,
                        PrecioUnitario = detalle.PrecioVentaUnitario.Value,
                        CantidadADevolver = 0, // Inicia en 0
                        Motivo = ""
                    });
                }

                // Suscribirse al evento PropertyChanged de CADA item
                foreach (var item in itemsParaDevolver)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }

                ActualizarTotalDevolucion(); // Calcular total inicial (que es 0)
            }
        }

        // Este evento se dispara cada vez que el usuario cambia la "Cantidad a Devolver"
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

            // 1. Filtrar solo los items que realmente se van a devolver
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
                dtDetalle.Rows.Add(
                    item.IdLote,
                    item.CantidadADevolver,
                    item.PrecioUnitario,
                    item.SubTotalDevolucion
                );
            }

            // 3. Crear el objeto Devolucion
            DEVOLUCIONES_CLIENTES nuevaDevolucion = new DEVOLUCIONES_CLIENTES()
            {
                IdVenta = ventaEncontrada.IdVenta,
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