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
    public partial class ModalProveedor : Window
    {
        private CN_Proveedor cn_proveedor = new CN_Proveedor();
        private PROVEEDORES proveedorActual;
        private bool esNuevo = false;

        // Constructor reutilizable
        public ModalProveedor(PROVEEDORES proveedor = null)
        {
            InitializeComponent();

            if (proveedor == null)
            {
                // Es un nuevo proveedor
                esNuevo = true;
                txtTitulo.Text = "✚ Nuevo Proveedor";
                proveedorActual = new PROVEEDORES();
                chkEstado.IsChecked = true;
            }
            else
            {
                // Es un proveedor existente (Modo Editar)
                esNuevo = false;
                txtTitulo.Text = "✏️ Editar Proveedor";
                proveedorActual = proveedor;

                // Cargar datos en los campos
                txtIdProveedor.Text = proveedor.IdProveedor.ToString();
                txtDocumento.Text = proveedor.Documento;
                txtRazonSocial.Text = proveedor.RazonSocial;
                txtCorreo.Text = proveedor.Correo;
                txtTelefono.Text = proveedor.Telefono;
                chkEstado.IsChecked = proveedor.Estado;
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = string.Empty;

            // 1. Llenar el objeto proveedorActual con los datos de los TextBox
            proveedorActual.Documento = txtDocumento.Text;
            proveedorActual.RazonSocial = txtRazonSocial.Text;
            proveedorActual.Correo = txtCorreo.Text;
            proveedorActual.Telefono = txtTelefono.Text;
            // Usamos '?? false' para convertir de forma segura el bool? a bool
            proveedorActual.Estado = chkEstado.IsChecked ?? false;

            if (esNuevo)
            {
                // ---- MODO REGISTRAR ----
                int idGenerado = cn_proveedor.Registrar(proveedorActual, out mensaje);

                if (idGenerado > 0)
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
            else
            {
                // ---- MODO EDITAR ----
                proveedorActual.IdProveedor = int.Parse(txtIdProveedor.Text);
                bool resultado = cn_proveedor.Editar(proveedorActual, out mensaje);

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
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}