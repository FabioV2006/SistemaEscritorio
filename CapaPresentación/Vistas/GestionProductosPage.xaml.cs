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
using System.Collections;

namespace CapaPresentación.Vistas
{
    public partial class GestionProductosPage : Page
    {
        private CN_Producto cn_producto = new CN_Producto();
        private CN_Categoria cn_categoria = new CN_Categoria();
        private CN_Laboratorio cn_laboratorio = new CN_Laboratorio();

        private List<PRODUCTOS> listaProductosCompleta;

        public GestionProductosPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatos();
            CargarFiltros();
        }

        private void CargarDatos()
        {
            listaProductosCompleta = cn_producto.Listar();
            dgProductos.ItemsSource = listaProductosCompleta;
        }

        private void CargarFiltros()
        {
            // Cargar Categorías
            var categorias = cn_categoria.Listar();
            cboFiltroCategoria.ItemsSource = categorias;
            cboFiltroCategoria.DisplayMemberPath = "Descripcion";
            cboFiltroCategoria.SelectedValuePath = "IdCategoria";

            // Cargar Laboratorios
            var laboratorios = cn_laboratorio.Listar();
            cboFiltroLaboratorio.ItemsSource = laboratorios;
            cboFiltroLaboratorio.DisplayMemberPath = "Nombre";
            cboFiltroLaboratorio.SelectedValuePath = "IdLaboratorio";
        }

        private void btnNuevoProducto_Click(object sender, RoutedEventArgs e)
        {
            ModalProducto modal = new ModalProducto(null);
            bool? resultado = modal.ShowDialog();
            if (resultado == true)
            {
                CargarDatos();
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            var listaFiltrada = listaProductosCompleta;

            // 1. Filtrar por texto
            if (!string.IsNullOrEmpty(txtBusqueda.Text))
            {
                string filtro = txtBusqueda.Text.ToLower();
                if ((cboFiltro.SelectedItem as ComboBoxItem)?.Content.ToString() == "Código")
                {
                    listaFiltrada = listaFiltrada.Where(p => p.Codigo.ToLower().Contains(filtro)).ToList();
                }
                else // Nombre
                {
                    listaFiltrada = listaFiltrada.Where(p => p.Nombre.ToLower().Contains(filtro)).ToList();
                }
            }

            // 2. Filtrar por Categoría
            if (cboFiltroCategoria.SelectedValue != null)
            {
                int idCategoria = (int)cboFiltroCategoria.SelectedValue;
                listaFiltrada = listaFiltrada.Where(p => p.IdCategoria == idCategoria).ToList();
            }

            // 3. Filtrar por Laboratorio
            if (cboFiltroLaboratorio.SelectedValue != null)
            {
                int idLaboratorio = (int)cboFiltroLaboratorio.SelectedValue;
                listaFiltrada = listaFiltrada.Where(p => p.IdLaboratorio == idLaboratorio).ToList();
            }

            dgProductos.ItemsSource = listaFiltrada;
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            PRODUCTOS productoSeleccionado = (sender as Button).DataContext as PRODUCTOS;
            if (productoSeleccionado == null) return;

            ModalProducto modal = new ModalProducto(productoSeleccionado);
            bool? resultado = modal.ShowDialog();
            if (resultado == true)
            {
                CargarDatos();
            }
        }

        private void btnEstado_Click(object sender, RoutedEventArgs e)
        {
            PRODUCTOS productoSeleccionado = (sender as Button).DataContext as PRODUCTOS;
            if (productoSeleccionado == null) return;

            string mensaje;
            string accion = (productoSeleccionado.Estado ?? false) ? "Desactivar" : "Activar";

            MessageBoxResult resultadoMsg = MessageBox.Show(
                $"¿Está seguro de que desea {accion} el producto: {productoSeleccionado.Nombre}?",
                $"Confirmar {accion}",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultadoMsg == MessageBoxResult.Yes)
            {
                productoSeleccionado.Estado = !(productoSeleccionado.Estado ?? false);
                bool resultado = cn_producto.Editar(productoSeleccionado, out mensaje);

                if (resultado)
                {
                    MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarDatos();
                }
                else
                {
                    MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    productoSeleccionado.Estado = !(productoSeleccionado.Estado ?? false);
                }
            }
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            string titulo = "Reporte_de_Productos";

            // Llama a la nueva función, pasándole el DataGrid
            ExportadorHelper.ExportarDataGridAExcel(dgProductos, titulo);
        }
    }
}