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
using CapaNegocio;

namespace CapaPresentación.Vistas
{
    public partial class DashboardPage : Page
    {
        private CN_Dashboard cn_dashboard = new CN_Dashboard();

        public DashboardPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Cuando la página cargue, llamamos a nuestro método
            CargarDatosKPIs();
        }

        private void CargarDatosKPIs()
        {
            try
            {
                // 1. Llamamos a la Capa de Negocio
                decimal ventasMes = cn_dashboard.ObtenerTotalVentasMes();
                int clientesActivos = cn_dashboard.ObtenerClientesActivos();
                int lotesVencer = cn_dashboard.ObtenerLotesPorVencer();
                int stockCritico = cn_dashboard.ObtenerStockCritico();

                // 2. Actualizamos la Interfaz de Usuario (UI)
                txtVentasMes.Text = ventasMes.ToString("C"); // "C" = Formato de Moneda
                txtClientesActivos.Text = clientesActivos.ToString();
                txtLotesVencer.Text = lotesVencer.ToString();
                txtStockCritico.Text = stockCritico.ToString();

                // 3. (Opcional) Lógica de colores para las alertas
                // Si no hay lotes por vencer, no mostramos el rojo.
                txtLotesVencer.Foreground = (lotesVencer > 0) ?
                    (SolidColorBrush)(new BrushConverter().ConvertFrom("#d9534f")) : Brushes.Gray;

                // Si no hay stock crítico, no mostramos el naranja.
                txtStockCritico.Foreground = (stockCritico > 0) ?
                    (SolidColorBrush)(new BrushConverter().ConvertFrom("#f0ad4e")) : Brushes.Gray;
            }
            catch (Exception ex)
            {
                // Si la base de datos falla, mostramos un error en lugar de crashear
                MessageBox.Show("Error al cargar el Dashboard: " + ex.Message, "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}