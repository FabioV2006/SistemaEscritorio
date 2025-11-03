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
    public partial class HistorialComprasPage : Page
    {
        private CN_Compra cn_compra = new CN_Compra();
        private List<COMPRAS> listaComprasCompleta;

        public HistorialComprasPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            listaComprasCompleta = cn_compra.Listar();
            dgCompras.ItemsSource = listaComprasCompleta;
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            var listaFiltrada = listaComprasCompleta;

            // 1. Filtrar por Fecha Inicio
            if (dpFechaInicio.SelectedDate.HasValue)
            {
                listaFiltrada = listaFiltrada.Where(c => c.FechaRegistro.Value.Date >= dpFechaInicio.SelectedDate.Value.Date).ToList();
            }

            // 2. Filtrar por Fecha Fin
            if (dpFechaFin.SelectedDate.HasValue)
            {
                listaFiltrada = listaFiltrada.Where(c => c.FechaRegistro.Value.Date <= dpFechaFin.SelectedDate.Value.Date).ToList();
            }

            // 3. Filtrar por Proveedor
            if (!string.IsNullOrEmpty(txtFiltroProveedor.Text))
            {
                string filtro = txtFiltroProveedor.Text.ToLower();
                listaFiltrada = listaFiltrada.Where(c => c.PROVEEDORES.RazonSocial.ToLower().Contains(filtro)).ToList();
            }

            dgCompras.ItemsSource = listaFiltrada;
        }

        private void btnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            // Obtener la compra de la fila seleccionada
            COMPRAS oCompra = (sender as Button).DataContext as COMPRAS;
            if (oCompra == null) return;

            // Abrir el modal de detalle, pasando el objeto COMPRAS completo
            ModalDetalleCompra modal = new ModalDetalleCompra(oCompra);
            modal.ShowDialog();
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Simulando exportación a Excel... 📄", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}