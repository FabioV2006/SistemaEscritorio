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
    public partial class ModalDetalleDevCliente : Window
    {
        private CN_DevolucionCliente cn_devolucion = new CN_DevolucionCliente();
        private DEVOLUCIONES_CLIENTES _devolucion;

        public ModalDetalleDevCliente(DEVOLUCIONES_CLIENTES oDevolucion)
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

            // Usamos los datos ya cargados por .Include() en la grilla principal
            txtCliente.Text = _devolucion.VENTAS?.CLIENTES?.RazonSocial ?? "N/A";
            txtNroDocVenta.Text = _devolucion.VENTAS?.NumeroDocumento ?? "N/A";

            // Llenar el monto
            txtMontoTotal.Text = $"Monto Total Devuelto: {_devolucion.MontoTotalDevuelto:C}";

            // 2. Cargar la grilla de detalle (haciendo una nueva consulta)
            List<DETALLE_DEVOLUCIONES_CLIENTES> detalle = cn_devolucion.ObtenerDetalle(_devolucion.IdDevolucionCliente);
            dgDetalle.ItemsSource = detalle;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}