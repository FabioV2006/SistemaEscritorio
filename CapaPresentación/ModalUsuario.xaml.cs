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
    public partial class ModalUsuario : Window
    {
        private CN_Usuario cn_usuario = new CN_Usuario();
        private CN_Rol cn_rol = new CN_Rol();

        private USUARIOS usuarioActual;
        private bool esNuevo = false;

        public ModalUsuario(USUARIOS usuario = null)
        {
            InitializeComponent();

            if (usuario == null)
            {
                // Es un nuevo usuario
                esNuevo = true;
                txtTitulo.Text = "✚ Nuevo Usuario";
                usuarioActual = new USUARIOS();
                chkEstado.IsChecked = true;
            }
            else
            {
                // Es un usuario existente (Modo Editar)
                esNuevo = false;
                txtTitulo.Text = "✏️ Editar Usuario";
                usuarioActual = usuario;

                // Cargar datos en los campos
                txtIdUsuario.Text = usuario.IdUsuario.ToString();
                txtDocumento.Text = usuario.Documento;
                txtNombre.Text = usuario.NombreCompleto;
                txtCorreo.Text = usuario.Correo;
                txtClave.Password = usuario.Clave; // Carga la clave para edición
                chkEstado.IsChecked = usuario.Estado;

                // El ComboBox se seleccionará en el evento Loaded
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarRoles();
        }

        private void CargarRoles()
        {
            var roles = cn_rol.Listar();
            cboRol.ItemsSource = roles;
            cboRol.DisplayMemberPath = "Descripcion";
            cboRol.SelectedValuePath = "IdRol";

            // Si estamos editando, seleccionar el rol actual
            if (!esNuevo)
            {
                cboRol.SelectedValue = usuarioActual.IdRol;
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = string.Empty;

            // 1. Llenar el objeto usuarioActual
            usuarioActual.Documento = txtDocumento.Text;
            usuarioActual.NombreCompleto = txtNombre.Text;
            usuarioActual.Correo = txtCorreo.Text;
            usuarioActual.Clave = txtClave.Password; // En un sistema real, encriptar/hashear

            if (cboRol.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar un Rol.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            usuarioActual.IdRol = (int)cboRol.SelectedValue;
            usuarioActual.Estado = chkEstado.IsChecked ?? false;

            if (esNuevo)
            {
                // ---- MODO REGISTRAR ----
                int idGenerado = cn_usuario.Registrar(usuarioActual, out mensaje);

                if (idGenerado > 0)
                {
                    MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
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
                usuarioActual.IdUsuario = int.Parse(txtIdUsuario.Text);
                bool resultado = cn_usuario.Editar(usuarioActual, out mensaje);

                if (resultado)
                {
                    MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
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