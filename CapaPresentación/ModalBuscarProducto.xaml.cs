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
    public partial class ModalBuscarProducto : Window
    {
        public PRODUCTOS ProductoSeleccionado { get; private set; }
        private CN_Producto cn_producto = new CN_Producto();
        private List<PRODUCTOS> listaProductos;

        public ModalBuscarProducto()
        {
            InitializeComponent();
            CargarProductos();
        }

        private void CargarProductos()
        {
            // Solo listamos productos activos
            listaProductos = cn_producto.Listar().Where(p => p.Estado == true).ToList();
            dgProductos.ItemsSource = listaProductos;
        }

        private void SeleccionarProducto()
        {
            if (dgProductos.SelectedItem != null)
            {
                ProductoSeleccionado = (PRODUCTOS)dgProductos.SelectedItem;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string filtro = txtBusqueda.Text.ToLower();
            if (string.IsNullOrEmpty(filtro))
            {
                dgProductos.ItemsSource = listaProductos;
                return;
            }

            var listaFiltrada = listaProductos;

            if (((ComboBoxItem)cboFiltro.SelectedItem).Content.ToString() == "Código")
            {
                listaFiltrada = listaProductos.Where(p => p.Codigo.ToLower().Contains(filtro)).ToList();
            }
            else // Nombre
            {
                listaFiltrada = listaProductos.Where(p => p.Nombre.ToLower().Contains(filtro)).ToList();
            }
            dgProductos.ItemsSource = listaFiltrada;
        }

        private void txtBusqueda_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnBuscar_Click(sender, e);
            }
        }

        private void dgProductos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SeleccionarProducto();
        }
    }
}