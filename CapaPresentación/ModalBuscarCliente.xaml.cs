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

namespace CapaPresentación
{
    public partial class ModalBuscarCliente : Window
    {
        public CLIENTES ClienteSeleccionado { get; private set; }
        private CN_Cliente cn_cliente = new CN_Cliente();
        private List<CLIENTES> listaClientes;

        public ModalBuscarCliente()
        {
            InitializeComponent();
            CargarClientes();
        }

        private void CargarClientes()
        {
            // Solo listamos clientes activos
            listaClientes = cn_cliente.Listar().Where(c => c.Estado == true).ToList();
            dgClientes.ItemsSource = listaClientes;
        }

        private void SeleccionarCliente()
        {
            if (dgClientes.SelectedItem != null)
            {
                ClienteSeleccionado = (CLIENTES)dgClientes.SelectedItem;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string filtro = txtBusqueda.Text.ToLower();
            if (string.IsNullOrEmpty(filtro))
            {
                dgClientes.ItemsSource = listaClientes;
                return;
            }

            var listaFiltrada = listaClientes;

            if (((ComboBoxItem)cboFiltro.SelectedItem).Content.ToString() == "Documento")
            {
                listaFiltrada = listaClientes.Where(c => c.Documento.Contains(filtro)).ToList();
            }
            else // Razón Social
            {
                listaFiltrada = listaClientes.Where(c => c.RazonSocial.ToLower().Contains(filtro)).ToList();
            }
            dgClientes.ItemsSource = listaFiltrada;
        }

        private void txtBusqueda_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) btnBuscar_Click(sender, e);
        }

        private void dgClientes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SeleccionarCliente();
        }
    }
}