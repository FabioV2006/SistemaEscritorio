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
    public partial class ModalBuscarVenta : Window
    {
        public VENTAS VentaSeleccionada { get; private set; }
        private CN_Venta cn_venta = new CN_Venta();
        private List<VENTAS> listaVentas;

        public ModalBuscarVenta()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarVentas();
        }

        private void CargarVentas()
        {
            // Usamos el Listar() que ya teníamos
            listaVentas = cn_venta.Listar();
            dgVentas.ItemsSource = listaVentas;
        }

        private void SeleccionarVenta()
        {
            if (dgVentas.SelectedItem != null)
            {
                VentaSeleccionada = (VENTAS)dgVentas.SelectedItem;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string filtro = txtBusqueda.Text.ToLower();
            if (string.IsNullOrEmpty(filtro))
            {
                dgVentas.ItemsSource = listaVentas;
                return;
            }

            var listaFiltrada = listaVentas;

            if (((ComboBoxItem)cboFiltro.SelectedItem).Content.ToString() == "Nro. Documento")
            {
                listaFiltrada = listaVentas.Where(v => v.NumeroDocumento.ToLower().Contains(filtro)).ToList();
            }
            else // Razón Social Cliente
            {
                // Usamos '?? new CLIENTES()' para evitar un error si un cliente es nulo
                listaFiltrada = listaVentas.Where(v => (v.CLIENTES ?? new CLIENTES()).RazonSocial.ToLower().Contains(filtro)).ToList();
            }
            dgVentas.ItemsSource = listaFiltrada;
        }

        private void txtBusqueda_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) btnBuscar_Click(sender, e);
        }

        private void dgVentas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SeleccionarVenta();
        }
    }
}