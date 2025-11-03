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
    public partial class ModalBuscarProveedor : Window
    {
        public PROVEEDORES ProveedorSeleccionado { get; private set; }
        private CN_Proveedor cn_proveedor = new CN_Proveedor();
        private List<PROVEEDORES> listaProveedores;

        public ModalBuscarProveedor()
        {
            InitializeComponent();
            CargarProveedores();
        }

        private void CargarProveedores()
        {
            listaProveedores = cn_proveedor.Listar().Where(p => p.Estado == true).ToList();
            dgProveedores.ItemsSource = listaProveedores;
        }

        private void SeleccionarProveedor()
        {
            if (dgProveedores.SelectedItem != null)
            {
                ProveedorSeleccionado = (PROVEEDORES)dgProveedores.SelectedItem;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string filtro = txtBusqueda.Text.ToLower();
            if (string.IsNullOrEmpty(filtro))
            {
                dgProveedores.ItemsSource = listaProveedores;
                return;
            }

            var listaFiltrada = listaProveedores;

            if (((ComboBoxItem)cboFiltro.SelectedItem).Content.ToString() == "Documento")
            {
                listaFiltrada = listaProveedores.Where(p => p.Documento.Contains(filtro)).ToList();
            }
            else // Razón Social
            {
                listaFiltrada = listaProveedores.Where(p => p.RazonSocial.ToLower().Contains(filtro)).ToList();
            }
            dgProveedores.ItemsSource = listaFiltrada;
        }

        private void txtBusqueda_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnBuscar_Click(sender, e);
            }
        }

        private void dgProveedores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SeleccionarProveedor();
        }
    }
}