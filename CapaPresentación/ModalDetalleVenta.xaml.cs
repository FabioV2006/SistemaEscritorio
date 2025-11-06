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

namespace CapaPresentación
{
    public partial class ModalDetalleVenta : Window
    {
        private CN_Venta cn_venta = new CN_Venta();
        private VENTAS _venta;
        private List<DETALLE_VENTAS> _detalles;

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
            _detalles = cn_venta.ObtenerDetalle(_venta.IdVenta);
            dgDetalle.ItemsSource = _detalles;
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_venta == null || _detalles == null)
                {
                    MessageBox.Show("No hay datos para imprimir.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Llamar a la utilidad de impresión
                ImpresionHelper.ImprimirVenta(_venta, _detalles);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}