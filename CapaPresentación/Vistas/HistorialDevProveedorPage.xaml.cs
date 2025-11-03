using System;
using System.Collections.Generic;
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
    public partial class HistorialDevProveedorPage : Page
    {
        private CN_DevolucionProveedor cn_devolucion = new CN_DevolucionProveedor();
        private List<DEVOLUCIONES_PROVEEDORES> listaDevolucionesCompleta;

        public HistorialDevProveedorPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            listaDevolucionesCompleta = cn_devolucion.Listar();
            dgDevoluciones.ItemsSource = listaDevolucionesCompleta;
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            var listaFiltrada = listaDevolucionesCompleta;

            // Filtros de Fecha
            if (dpFechaInicio.SelectedDate.HasValue)
            {
                listaFiltrada = listaFiltrada.Where(d => d.FechaRegistro.Value.Date >= dpFechaInicio.SelectedDate.Value.Date).ToList();
            }
            if (dpFechaFin.SelectedDate.HasValue)
            {
                listaFiltrada = listaFiltrada.Where(d => d.FechaRegistro.Value.Date <= dpFechaFin.SelectedDate.Value.Date).ToList();
            }

            // Filtro por Proveedor
            if (!string.IsNullOrEmpty(txtFiltroProveedor.Text))
            {
                string filtro = txtFiltroProveedor.Text.ToLower();
                listaFiltrada = listaFiltrada.Where(d => (d.COMPRAS.PROVEEDORES ?? new PROVEEDORES()).RazonSocial.ToLower().Contains(filtro)).ToList();
            }

            dgDevoluciones.ItemsSource = listaFiltrada;
        }

        private void btnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            DEVOLUCIONES_PROVEEDORES oDevolucion = (sender as Button).DataContext as DEVOLUCIONES_PROVEEDORES;
            if (oDevolucion == null) return;

            ModalDetalleDevProveedor modal = new ModalDetalleDevProveedor(oDevolucion);
            modal.ShowDialog();
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Simulando exportación a Excel... 📄", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}