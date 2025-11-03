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
    public partial class ModalDetalleVenta : Window
    {
        private CN_Venta cn_venta = new CN_Venta();
        private VENTAS _venta;

        public ModalDetalleVenta(VENTAS oVenta)
        {
            InitializeComponent();
            _venta = oVenta;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            if (_venta == null) return;

            // 1. Llenar los campos de texto
            txtNroDocumento.Text = _venta.NumeroDocumento;
            txtTipoDocumento.Text = _venta.TipoDocumento;
            txtFecha.Text = _venta.FechaRegistro?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

            // Usamos los datos ya cargados por .Include() en la grilla principal
            txtRazonSocialCliente.Text = _venta.CLIENTES?.RazonSocial ?? "N/A";
            txtDocumentoCliente.Text = _venta.CLIENTES?.Documento ?? "N/A";

            // Llenar los montos
            txtMontoTotal.Text = $"{_venta.MontoTotal:C}";
            txtMontoPago.Text = $"{_venta.MontoPago:C}";
            txtMontoCambio.Text = $"{_venta.MontoCambio:C}";

            // 2. Cargar la grilla de detalle (haciendo una nueva consulta)
            List<DETALLE_VENTAS> detalle = cn_venta.ObtenerDetalle(_venta.IdVenta);
            dgDetalle.ItemsSource = detalle;
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Simulando impresión de: {_venta.NumeroDocumento}", "Imprimir", MessageBoxButton.OK, MessageBoxImage.Information);
            // Aquí llamarías a tu lógica de generación de PDF o impresión
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}