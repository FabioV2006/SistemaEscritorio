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

namespace CapaPresentación
{
    public partial class ModalReporteVentas : Window
    {
        private List<DETALLE_VENTAS> _datosReporte;

        public ModalReporteVentas(List<DETALLE_VENTAS> datos, DateTime fechaInicio, DateTime fechaFin)
        {
            InitializeComponent();
            _datosReporte = datos;

            // Actualizar el título con las fechas
            txtTitulo.Text = $"Reporte de Ventas ({fechaInicio:dd/MM/yy} al {fechaFin:dd/MM/yy})";

            // Cargar la grilla
            dgReporte.ItemsSource = _datosReporte;

            // Calcular y mostrar el total
            decimal totalGeneral = _datosReporte.Sum(d => d.SubTotal ?? 0);
            txtTotalGeneral.Text = $"{totalGeneral:C}";
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            // Aquí iría tu lógica de exportación (ej. con EPPlus)
            // Por ahora, mostramos un mensaje
            MessageBox.Show("Simulando exportación a Excel...", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}