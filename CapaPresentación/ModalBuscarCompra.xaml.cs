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
    public partial class ModalBuscarCompra : Window
    {
        public COMPRAS CompraSeleccionada { get; private set; }
        private CN_Compra cn_compra = new CN_Compra();
        private List<COMPRAS> listaCompras;

        public ModalBuscarCompra()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarCompras();
        }

        private void CargarCompras()
        {
            listaCompras = cn_compra.Listar();
            dgCompras.ItemsSource = listaCompras;
        }

        private void SeleccionarCompra()
        {
            if (dgCompras.SelectedItem != null)
            {
                CompraSeleccionada = (COMPRAS)dgCompras.SelectedItem;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string filtro = txtBusqueda.Text.ToLower();
            if (string.IsNullOrEmpty(filtro))
            {
                dgCompras.ItemsSource = listaCompras;
                return;
            }

            var listaFiltrada = listaCompras;

            if (((ComboBoxItem)cboFiltro.SelectedItem).Content.ToString() == "Nro. Documento")
            {
                listaFiltrada = listaCompras.Where(c => c.NumeroDocumento.ToLower().Contains(filtro)).ToList();
            }
            else 
            {
                listaFiltrada = listaCompras.Where(c => (c.PROVEEDORES ?? new PROVEEDORES()).RazonSocial.ToLower().Contains(filtro)).ToList();
            }
            dgCompras.ItemsSource = listaFiltrada;
        }

        private void txtBusqueda_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) btnBuscar_Click(sender, e);
        }

        private void dgCompras_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SeleccionarCompra();
        }
    }
}