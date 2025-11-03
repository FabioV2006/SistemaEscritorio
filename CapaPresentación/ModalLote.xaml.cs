using System;
using System.Collections.Generic;
using System.Globalization;
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
    public partial class ModalLote : Window
    {
        private CN_Lote cn_lote = new CN_Lote();
        private LOTES loteActual; // Siempre estamos editando

        public ModalLote(LOTES lote)
        {
            InitializeComponent();

            if (lote == null)
            {
                // Esto no debería pasar desde la grilla, pero es una salvaguarda
                MessageBox.Show("Error: No se ha seleccionado ningún lote.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            loteActual = lote;

            // Cargar datos (algunos read-only, otros editables)
            txtIdLote.Text = lote.IdLote.ToString();
            txtProducto.Text = lote.PRODUCTOS?.Nombre ?? "Producto no encontrado"; // El '?' es un null-check
            txtLote.Text = lote.NumeroLote;
            txtVencimiento.Text = lote.FechaVencimiento.ToString("dd/MM/yyyy");
            txtPrecioCompra.Text = lote.PrecioCompra.ToString("F2"); // "F2" = 2 decimales

            // Campos editables
            txtPrecioVenta.Text = lote.PrecioVenta.ToString("F2");
            txtStock.Text = lote.Stock.ToString();
            txtUbicacion.Text = lote.UbicacionAlmacen;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = string.Empty;

            // 1. Validar y parsear los campos editables
            if (!decimal.TryParse(txtPrecioVenta.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal precioVenta) && !decimal.TryParse(txtPrecioVenta.Text, out precioVenta))
            {
                if (!decimal.TryParse(txtPrecioVenta.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out precioVenta))
                {
                    MessageBox.Show("El formato del Precio de Venta no es válido.", "Error de Formato", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (!int.TryParse(txtStock.Text, out int stock))
            {
                MessageBox.Show("El formato del Stock no es válido. Debe ser un número entero.", "Error de Formato", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2. Llenar el objeto loteActual con los datos de los TextBox
            loteActual.PrecioVenta = precioVenta;
            loteActual.Stock = stock;
            loteActual.UbicacionAlmacen = txtUbicacion.Text;

            // 3. Llamar a la Capa de Negocio
            bool resultado = cn_lote.Editar(loteActual, out mensaje);

            if (resultado)
            {
                MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; // Avisa a la página anterior que debe recargar
                this.Close();
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}