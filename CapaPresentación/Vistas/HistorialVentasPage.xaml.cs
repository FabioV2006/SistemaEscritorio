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
using CapaPresentación.Utilidades;

namespace CapaPresentación.Vistas
{
    public partial class HistorialVentasPage : Page
    {
        private CN_Venta cn_venta = new CN_Venta();
        private List<VENTAS> listaVentasCompleta;

        public HistorialVentasPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            listaVentasCompleta = cn_venta.Listar();
            dgVentas.ItemsSource = listaVentasCompleta;
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            var listaFiltrada = listaVentasCompleta;

            // 1. Filtrar por Fecha Inicio
            if (dpFechaInicio.SelectedDate.HasValue)
            {
                listaFiltrada = listaFiltrada.Where(v => v.FechaRegistro.Value.Date >= dpFechaInicio.SelectedDate.Value.Date).ToList();
            }

            // 2. Filtrar por Fecha Fin
            if (dpFechaFin.SelectedDate.HasValue)
            {
                listaFiltrada = listaFiltrada.Where(v => v.FechaRegistro.Value.Date <= dpFechaFin.SelectedDate.Value.Date).ToList();
            }

            // 3. Filtrar por Cliente (Razón Social)
            if (!string.IsNullOrEmpty(txtFiltroCliente.Text))
            {
                string filtro = txtFiltroCliente.Text.ToLower();
                listaFiltrada = listaFiltrada.Where(v => v.CLIENTES.RazonSocial.ToLower().Contains(filtro)).ToList();
            }

            dgVentas.ItemsSource = listaFiltrada;
        }

        private void btnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            // Obtener la venta de la fila seleccionada
            VENTAS oVenta = (sender as Button).DataContext as VENTAS;
            if (oVenta == null) return;

            // Abrir el modal de detalle, pasando el objeto VENTAS completo
            ModalDetalleVenta modal = new ModalDetalleVenta(oVenta);
            modal.ShowDialog();
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            string titulo = "Reporte_de_Ventas";

            // Llama a la nueva función, pasándole el DataGrid
            ExportadorHelper.ExportarDataGridAExcel(dgVentas, titulo);
        }
    }
}