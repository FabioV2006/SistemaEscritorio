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
    public partial class GestionLotesPage : Page
    {
        private CN_Lote cn_lote = new CN_Lote();
        private List<LOTES> listaLotesCompleta;

        public GestionLotesPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            listaLotesCompleta = cn_lote.Listar();
            dgLotes.ItemsSource = listaLotesCompleta;
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            // Lógica de filtro
            var listaFiltrada = listaLotesCompleta;

            string filtroProducto = txtFiltroProducto.Text.ToLower();
            string filtroLote = txtFiltroLote.Text.ToLower();

            // 1. Filtrar por Producto
            if (!string.IsNullOrEmpty(filtroProducto))
            {
                listaFiltrada = listaFiltrada
                    .Where(l => l.PRODUCTOS.Nombre.ToLower().Contains(filtroProducto))
                    .ToList();
            }

            // 2. Filtrar por Lote
            if (!string.IsNullOrEmpty(filtroLote))
            {
                listaFiltrada = listaFiltrada
                    .Where(l => l.NumeroLote.ToLower().Contains(filtroLote))
                    .ToList();
            }

            // 3. Filtrar por Fecha de Vencimiento
            if (dpFiltroVencimiento.SelectedDate.HasValue)
            {
                DateTime fechaFiltro = dpFiltroVencimiento.SelectedDate.Value;
                listaFiltrada = listaFiltrada
                    .Where(l => l.FechaVencimiento.Date <= fechaFiltro.Date)
                    .ToList();
            }

            dgLotes.ItemsSource = listaFiltrada;
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            LOTES loteSeleccionado = (sender as Button).DataContext as LOTES;
            if (loteSeleccionado == null) return;

            // Abrir el modal en modo "Editar"
            ModalLote modal = new ModalLote(loteSeleccionado);
            bool? resultado = modal.ShowDialog();

            if (resultado == true)
            {
                CargarDatos(); // Recargar la grilla si se guardó
            }
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            string titulo = "Reporte_de_Lotes";

            // Llama a la nueva función, pasándole el DataGrid
            ExportadorHelper.ExportarDataGridAExcel(dgLotes, titulo);
        }
    }
}