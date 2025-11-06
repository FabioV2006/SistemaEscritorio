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
    public partial class ModalDetalleCompra : Window
    {
        private CN_Compra cn_compra = new CN_Compra();
        private COMPRAS _compra;
        private List<DETALLE_COMPRAS> _detalles;

        public ModalDetalleCompra(COMPRAS oCompra)
        {
            InitializeComponent();
            _compra = oCompra;
            CargarDatos();
        }

        private void CargarDatos()
        {
            if (_compra == null) return;

            // 1. Llenar los campos de texto
            txtNroDocumento.Text = _compra.NumeroDocumento;
            txtTipoDocumento.Text = _compra.TipoDocumento;
            txtFecha.Text = _compra.FechaRegistro?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

            // Usamos los datos ya cargados por .Include()
            txtRazonSocialProv.Text = _compra.PROVEEDORES?.RazonSocial ?? "N/A";
            txtDocumentoProv.Text = _compra.PROVEEDORES?.Documento ?? "N/A";

            txtMontoTotal.Text = $"Monto Total: {_compra.MontoTotal:C}";

            // 2. Cargar la grilla de detalle (haciendo una nueva consulta)
            _detalles = cn_compra.ObtenerDetalle(_compra.IdCompra);
            dgDetalle.ItemsSource = _detalles;
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_compra == null || _detalles == null)
                {
                    MessageBox.Show("No hay datos para imprimir.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Llamar a la utilidad de impresión
                ImpresionHelper.ImprimirCompra(_compra, _detalles);
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