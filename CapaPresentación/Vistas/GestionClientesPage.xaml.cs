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
    public partial class GestionClientesPage : Page
    {
        private CN_Cliente cn_cliente = new CN_Cliente();
        private List<CLIENTES> listaClientesCompleta; // Para filtros

        public GestionClientesPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            listaClientesCompleta = cn_cliente.Listar();
            dgClientes.ItemsSource = listaClientesCompleta;
        }

        private void btnNuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            // Abrir el modal en modo "Crear"
            // Pasamos 'null' para indicar que es un nuevo cliente
            ModalCliente modal = new ModalCliente(null);
            bool? resultado = modal.ShowDialog();

            // Si el modal se cerró con "Guardar" (true), recargamos la lista
            if (resultado == true)
            {
                CargarDatos();
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            // Lógica de filtro (simplificada)
            var filtro = txtBusqueda.Text.ToLower();
            var estado = (cboEstado.SelectedItem as ComboBoxItem)?.Content.ToString();

            var listaFiltrada = listaClientesCompleta;

            // 1. Filtrar por estado
            if (estado == "Activo")
            {
                listaFiltrada = listaFiltrada.Where(c => c.Estado == true).ToList();
            }
            else if (estado == "Inactivo")
            {
                listaFiltrada = listaFiltrada.Where(c => c.Estado == false).ToList();
            }

            // 2. Filtrar por búsqueda
            if (!string.IsNullOrEmpty(filtro))
            {
                if ((cboFiltro.SelectedItem as ComboBoxItem)?.Content.ToString() == "Documento (RUC)")
                {
                    listaFiltrada = listaFiltrada.Where(c => c.Documento.Contains(filtro)).ToList();
                }
                else // Razón Social
                {
                    listaFiltrada = listaFiltrada.Where(c => c.RazonSocial.ToLower().Contains(filtro)).ToList();
                }
            }

            dgClientes.ItemsSource = listaFiltrada;
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            // Obtener el cliente de la fila seleccionada
            CLIENTES clienteSeleccionado = (sender as Button).DataContext as CLIENTES;

            if (clienteSeleccionado == null) return;

            // Abrir el modal en modo "Editar"
            // Pasamos el cliente seleccionado para que el modal cargue sus datos
            ModalCliente modal = new ModalCliente(clienteSeleccionado);
            bool? resultado = modal.ShowDialog();

            if (resultado == true)
            {
                CargarDatos();
            }
        }

        private void btnEstado_Click(object sender, RoutedEventArgs e)
        {
            CLIENTES clienteSeleccionado = (sender as Button).DataContext as CLIENTES;
            if (clienteSeleccionado == null) return;

            string mensaje;

            // ----- CORRECCIÓN 1 -----
            // Usamos (clienteSeleccionado.Estado ?? false)
            // Esto significa: "usa el valor de Estado. Si es nulo, usa 'false' en su lugar".
            string accion = (clienteSeleccionado.Estado ?? false) ? "Desactivar" : "Activar";

            MessageBoxResult resultadoMsg = MessageBox.Show(
                $"¿Está seguro de que desea {accion} al cliente: {clienteSeleccionado.RazonSocial}?",
                $"Confirmar {accion}",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultadoMsg == MessageBoxResult.Yes)
            {
                // ----- CORRECCIÓN 2 -----
                // Hacemos lo mismo aquí al invertir el valor
                clienteSeleccionado.Estado = !(clienteSeleccionado.Estado ?? false);

                bool resultado = cn_cliente.Editar(clienteSeleccionado, out mensaje);

                if (resultado)
                {
                    MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarDatos();
                }
                else
                {
                    MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Revertir el cambio si la BD falló
                    clienteSeleccionado.Estado = !(clienteSeleccionado.Estado ?? false);
                }
            }
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            string titulo = "Reporte_de_Clientes";

            // Llama a la nueva función, pasándole el DataGrid
            ExportadorHelper.ExportarDataGridAExcel(dgClientes, titulo);
        }
    }
}