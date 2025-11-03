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
    public partial class ModalDetalleDevProveedor : Window
    {
        private CN_DevolucionProveedor cn_devolucion = new CN_DevolucionProveedor();
        private DEVOLUCIONES_PROVEEDORES _devolucion;

        public ModalDetalleDevProveedor(DEVOLUCIONES_PROVEEDORES oDevolucion)
        {
            InitializeComponent();
            _devolucion = oDevolucion;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            if (_devolucion == null) return;

            // 1. Llenar los campos de texto
            txtFechaDev.Text = _devolucion.FechaRegistro?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
            txtUsuario.Text = _devolucion.USUARIOS?.NombreCompleto ?? "N/A";
            txtMotivo.Text = _devolucion.Motivo;

            txtProveedor.Text = _devolucion.COMPRAS?.PROVEEDORES?.RazonSocial ?? "N/A";
            txtNroDocCompra.Text = _devolucion.COMPRAS?.NumeroDocumento ?? "N/A";

            txtMontoTotal.Text = $"Monto Total Devuelto: {_devolucion.MontoTotalDevuelto:C}";

            // 2. Cargar la grilla de detalle
            List<DETALLE_DEVOLUCIONES_PROVEEDORES> detalle = cn_devolucion.ObtenerDetalle(_devolucion.IdDevolucionProveedor);
            dgDetalle.ItemsSource = detalle;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}