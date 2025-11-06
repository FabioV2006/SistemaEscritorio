using System;
using System.Collections.Generic;
using System.ComponentModel;
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
// --- Añade los usings de LiveCharts v0 ---
using LiveCharts;
using LiveCharts.Defaults; // Para ChartValues
using LiveCharts.Wpf;

namespace CapaPresentación.Vistas
{
    /// <summary>
    /// Lógica de interacción para DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page, INotifyPropertyChanged
    {
        private CN_Dashboard cnDashboard = new CN_Dashboard();

        // --- Propiedades para los Gráficos (Bindings de v0) ---
        public SeriesCollection VentasSeries { get; set; }
        public List<string> VentasXLabels { get; set; }
        public SeriesCollection TopProductosSeries { get; set; }
        public SeriesCollection VentasCategoriaSeries { get; set; }
        public Func<double, string> FormatoMoneda { get; set; } // Formateador para el eje Y

        public DashboardPage()
        {
            InitializeComponent();

            // --- Inicializa las colecciones de los gráficos ---
            VentasSeries = new SeriesCollection();
            VentasXLabels = new List<string>();
            TopProductosSeries = new SeriesCollection();
            VentasCategoriaSeries = new SeriesCollection();

            // Define el formateador de moneda (para el eje Y de ventas)
            FormatoMoneda = value => value.ToString("C"); // "C" es formato Moneda (ej. S/ 1,23.45)

            // Establece el DataContext de la página a sí misma para los bindings
            this.DataContext = this;
        }

        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Carga tus KPIs originales
            CargarDatosDashboard();

            // 2. Establece el rango de fechas default (últimos 3 meses)
            dpFechaFin.SelectedDate = DateTime.Now;
            dpFechaInicio.SelectedDate = DateTime.Now.AddMonths(-3);

            // 3. Carga los gráficos con el rango default
            CargarGraficos();
        }

        // Este es tu método original para cargar las tarjetas KPI
        private void CargarDatosDashboard()
        {
            try
            {
                txtTotalVentas.Text = cnDashboard.ObtenerTotalVentasMes().ToString("0.00");
                txtClientesActivos.Text = cnDashboard.ObtenerClientesActivos().ToString();
                txtLotesPorVencer.Text = cnDashboard.ObtenerLotesPorVencer().ToString();
                txtStockCritico.Text = cnDashboard.ObtenerStockCritico().ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar KPIs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Evento del botón para aplicar el filtro de fecha
        private void btnAplicarFiltro_Click(object sender, RoutedEventArgs e)
        {
            if (dpFechaInicio.SelectedDate == null || dpFechaFin.SelectedDate == null)
            {
                MessageBox.Show("Por favor, seleccione una fecha de inicio y fin.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (dpFechaInicio.SelectedDate > dpFechaFin.SelectedDate)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor a la fecha de fin.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CargarGraficos();
        }

        // Nuevo método para cargar todos los gráficos
        private void CargarGraficos()
        {
            // Tomamos el inicio del día seleccionado
            DateTime fechaInicio = dpFechaInicio.SelectedDate.Value.Date;
            // Tomamos el final del día seleccionado (23:59:59)
            DateTime fechaFin = dpFechaFin.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                // --- 1. Cargar Gráfico de Ventas Mensuales (Gráfico de Columnas) ---
                var ventasData = cnDashboard.ObtenerVentasMensuales(fechaInicio, fechaFin);

                VentasSeries.Clear(); // Limpia datos anteriores
                VentasSeries.Add(new ColumnSeries
                {
                    Title = "Total Ventas",
                    Values = new ChartValues<decimal>(ventasData.Select(v => v.TotalVendido))
                });

                VentasXLabels.Clear();
                VentasXLabels.AddRange(ventasData.Select(v => v.MesAnio));

                OnPropertyChanged(nameof(VentasSeries));
                OnPropertyChanged(nameof(VentasXLabels)); // Notifica el cambio de las etiquetas del eje X

                // --- 2. Cargar Gráfico Top Productos (Gráfico de Torta) ---
                var topProductosData = cnDashboard.ObtenerTopProductos(fechaInicio, fechaFin);

                TopProductosSeries.Clear(); // Limpia datos anteriores
                foreach (var producto in topProductosData)
                {
                    TopProductosSeries.Add(new PieSeries
                    {
                        Title = producto.Producto,
                        Values = new ChartValues<int>(new[] { producto.Cantidad }),
                        DataLabels = true,
                        // Formato de la etiqueta: "NombreProducto (Cantidad)"
                        LabelPoint = chartPoint => string.Format("{0} ({1:N0})", chartPoint.SeriesView.Title, chartPoint.Y)
                    });
                }
                OnPropertyChanged(nameof(TopProductosSeries));

                // --- 3. Cargar Gráfico Ventas por Categoría (Gráfico de Torta) ---
                var ventasCategoriaData = cnDashboard.ObtenerVentasCategoria(fechaInicio, fechaFin);

                VentasCategoriaSeries.Clear(); // Limpia datos anteriores
                foreach (var categoria in ventasCategoriaData)
                {
                    VentasCategoriaSeries.Add(new PieSeries
                    {
                        Title = categoria.Categoria,
                        Values = new ChartValues<decimal>(new[] { categoria.TotalVendido }),
                        DataLabels = true,
                        // Formato de la etiqueta: "NombreCategoria (S/ 1,234.56)"
                        LabelPoint = chartPoint => string.Format("{0} ({1:C2})", chartPoint.SeriesView.Title, chartPoint.Y)
                    });
                }
                OnPropertyChanged(nameof(VentasCategoriaSeries));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los gráficos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}