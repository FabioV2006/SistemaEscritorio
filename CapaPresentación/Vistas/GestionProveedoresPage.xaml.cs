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
    public partial class GestionProveedoresPage : Page
    {
        private CN_Proveedor cn_proveedor = new CN_Proveedor();
        private List<PROVEEDORES> listaProveedoresCompleta;

        public GestionProveedoresPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            listaProveedoresCompleta = cn_proveedor.Listar();
            dgProveedores.ItemsSource = listaProveedoresCompleta;
        }

        private void btnNuevoProveedor_Click(object sender, RoutedEventArgs e)
        {
            // Abrir el modal en modo "Crear"
            ModalProveedor modal = new ModalProveedor(null);
            bool? resultado = modal.ShowDialog();

            if (resultado == true)
            {
                CargarDatos();
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            var filtro = txtBusqueda.Text.ToLower();
            var estado = (cboEstado.SelectedItem as ComboBoxItem)?.Content.ToString();

            var listaFiltrada = listaProveedoresCompleta;

            // 1. Filtrar por estado
            if (estado == "Activo")
            {
                listaFiltrada = listaFiltrada.Where(p => p.Estado == true).ToList();
            }
            else if (estado == "Inactivo")
            {
                listaFiltrada = listaFiltrada.Where(p => p.Estado == false).ToList();
            }

            // 2. Filtrar por búsqueda
            if (!string.IsNullOrEmpty(filtro))
            {
                if ((cboFiltro.SelectedItem as ComboBoxItem)?.Content.ToString() == "Documento (RUC)")
                {
                    listaFiltrada = listaFiltrada.Where(p => p.Documento.Contains(filtro)).ToList();
                }
                else // Razón Social
                {
                    listaFiltrada = listaFiltrada.Where(p => p.RazonSocial.ToLower().Contains(filtro)).ToList();
                }
            }

            dgProveedores.ItemsSource = listaFiltrada;
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            PROVEEDORES proveedorSeleccionado = (sender as Button).DataContext as PROVEEDORES;
            if (proveedorSeleccionado == null) return;

            // Abrir el modal en modo "Editar"
            ModalProveedor modal = new ModalProveedor(proveedorSeleccionado);
            bool? resultado = modal.ShowDialog();

            if (resultado == true)
            {
                CargarDatos();
            }
        }

        private void btnEstado_Click(object sender, RoutedEventArgs e)
        {
            PROVEEDORES proveedorSeleccionado = (sender as Button).DataContext as PROVEEDORES;
            if (proveedorSeleccionado == null) return;

            string mensaje;
            // Usamos (proveedorSeleccionado.Estado ?? false) para manejar el bool?
            string accion = (proveedorSeleccionado.Estado ?? false) ? "Desactivar" : "Activar";

            MessageBoxResult resultadoMsg = MessageBox.Show(
                $"¿Está seguro de que desea {accion} al proveedor: {proveedorSeleccionado.RazonSocial}?",
                $"Confirmar {accion}",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultadoMsg == MessageBoxResult.Yes)
            {
                // Invertir el estado
                proveedorSeleccionado.Estado = !(proveedorSeleccionado.Estado ?? false);

                bool resultado = cn_proveedor.Editar(proveedorSeleccionado, out mensaje);

                if (resultado)
                {
                    MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarDatos();
                }
                else
                {
                    MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Revertir el cambio si la BD falló
                    proveedorSeleccionado.Estado = !(proveedorSeleccionado.Estado ?? false);
                }
            }
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            string titulo = "Reporte_de_Proveedores";

            // Llama a la nueva función, pasándole el DataGrid
            ExportadorHelper.ExportarDataGridAExcel(dgProveedores, titulo);
        }
    }
}