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
    public partial class ReportesPage : Page
    {
        // Necesitamos CN_Cliente para llenar el ComboBox de filtros
        private CN_Cliente cn_cliente = new CN_Cliente();
        private CN_Reporte cn_reporte = new CN_Reporte();

        public ReportesPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CargarFiltrosVentas();
        }

        private void CargarFiltrosVentas()
        {
            // Setear fechas por defecto (ej. el último mes)
            dpVentas_FechaFin.SelectedDate = DateTime.Now;
            dpVentas_FechaInicio.SelectedDate = DateTime.Now.AddMonths(-1);

            // Cargar ComboBox de Clientes
            var clientes = cn_cliente.Listar().Where(c => c.Estado == true).ToList();

            // Añadir un item "Todos" al principio
            clientes.Insert(0, new CLIENTES { IdCliente = 0, RazonSocial = "[Todos los Clientes]" });

            cboVentas_Cliente.ItemsSource = clientes;
            cboVentas_Cliente.DisplayMemberPath = "RazonSocial";
            cboVentas_Cliente.SelectedValuePath = "IdCliente";
            cboVentas_Cliente.SelectedIndex = 0; // Seleccionar "[Todos]"
        }

        private void btnGenerarReporteVentas_Click(object sender, RoutedEventArgs e)
        {
            if (dpVentas_FechaInicio.SelectedDate == null || dpVentas_FechaFin.SelectedDate == null)
            {
                MessageBox.Show("Debe seleccionar un rango de fechas.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fechaInicio = dpVentas_FechaInicio.SelectedDate.Value;
            DateTime fechaFin = dpVentas_FechaFin.SelectedDate.Value;
            int idCliente = (int)cboVentas_Cliente.SelectedValue;

            // 1. Llamar a la Capa de Negocio para obtener los datos
            List<DETALLE_VENTAS> datosReporte = cn_reporte.ReporteVentas(fechaInicio, fechaFin, idCliente);

            if (datosReporte.Count == 0)
            {
                MessageBox.Show("No se encontraron ventas para los filtros seleccionados.", "Sin Resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 2. Abrir el modal y pasarle los datos
            ModalReporteVentas modal = new ModalReporteVentas(datosReporte, fechaInicio, fechaFin);
            modal.ShowDialog();
        }
    }
}