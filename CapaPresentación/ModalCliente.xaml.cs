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
    public partial class ModalCliente : Window
    {
        private CN_Cliente cn_cliente = new CN_Cliente();
        private CLIENTES clienteActual; // Para saber si estamos editando
        private bool esNuevo = false;

        // Constructor para "Nuevo Cliente"
        public ModalCliente(CLIENTES cliente = null)
        {
            InitializeComponent();

            if (cliente == null)
            {
                // Es un nuevo cliente
                esNuevo = true;
                txtTitulo.Text = "✚ Nuevo Cliente";
                clienteActual = new CLIENTES(); // Creamos un objeto vacío
                chkEstado.IsChecked = true;
            }
            else
            {
                // Es un cliente existente (Modo Editar)
                esNuevo = false;
                txtTitulo.Text = "✏️ Editar Cliente";
                clienteActual = cliente;

                // Cargar datos en los campos
                txtIdCliente.Text = cliente.IdCliente.ToString();
                txtDocumento.Text = cliente.Documento;
                txtRazonSocial.Text = cliente.RazonSocial;
                txtNombreComercial.Text = cliente.NombreComercial;
                txtCorreo.Text = cliente.Correo;
                txtTelefono.Text = cliente.Telefono;
                txtDireccion.Text = cliente.DireccionEnvio;
                chkEstado.IsChecked = cliente.Estado;
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = string.Empty;

            // 1. Llenar el objeto clienteActual con los datos de los TextBox
            clienteActual.Documento = txtDocumento.Text;
            clienteActual.RazonSocial = txtRazonSocial.Text;
            clienteActual.NombreComercial = txtNombreComercial.Text;
            clienteActual.Correo = txtCorreo.Text;
            clienteActual.Telefono = txtTelefono.Text;
            clienteActual.DireccionEnvio = txtDireccion.Text;

            // ----- CORRECCIÓN -----
            // Un CheckBox.IsChecked también es 'bool?'. 
            // Usamos '?? false' para convertirlo de forma segura a 'bool'.
            clienteActual.Estado = chkEstado.IsChecked ?? false;

            if (esNuevo)
            {
                // ---- MODO REGISTRAR ----
                int idGenerado = cn_cliente.Registrar(clienteActual, out mensaje);

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
                clienteActual.IdCliente = int.Parse(txtIdCliente.Text); // Ya tenemos el ID
                bool resultado = cn_cliente.Editar(clienteActual, out mensaje);

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