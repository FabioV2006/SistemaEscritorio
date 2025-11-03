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
    public partial class HistorialDevClientePage : Page
    {
        private CN_DevolucionCliente cn_devolucion = new CN_DevolucionCliente();
        private List<DEVOLUCIONES_CLIENTES> listaDevolucionesCompleta;

        public HistorialDevClientePage()
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

            // 1. Filtrar por Fecha Inicio
            if (dpFechaInicio.SelectedDate.HasValue)
            {
                listaFiltrada = listaFiltrada.Where(d => d.FechaRegistro.Value.Date >= dpFechaInicio.SelectedDate.Value.Date).ToList();
            }

            // 2. Filtrar por Fecha Fin
            if (dpFechaFin.SelectedDate.HasValue)
            {
                listaFiltrada = listaFiltrada.Where(d => d.FechaRegistro.Value.Date <= dpFechaFin.SelectedDate.Value.Date).ToList();
            }

            // 3. Filtrar por Cliente (Razón Social)
            if (!string.IsNullOrEmpty(txtFiltroCliente.Text))
            {
                string filtro = txtFiltroCliente.Text.ToLower();
                // Navegamos por las relaciones: Devolucion -> Venta -> Cliente
                listaFiltrada = listaFiltrada.Where(d => (d.VENTAS.CLIENTES ?? new CLIENTES()).RazonSocial.ToLower().Contains(filtro)).ToList();
            }

            dgDevoluciones.ItemsSource = listaFiltrada;
        }

        private void btnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            DEVOLUCIONES_CLIENTES oDevolucion = (sender as Button).DataContext as DEVOLUCIONES_CLIENTES;
            if (oDevolucion == null) return;

            // Abrir el modal de detalle
            ModalDetalleDevCliente modal = new ModalDetalleDevCliente(oDevolucion);
            modal.ShowDialog();
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Simulando exportación a Excel... 📄", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}