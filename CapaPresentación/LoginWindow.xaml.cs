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
    public partial class LoginWindow : Window
    {
        public USUARIOS UsuarioLogueado { get; private set; }
        private CN_Usuario cn_usuario = new CN_Usuario();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnIngresar_Click(object sender, RoutedEventArgs e)
        {
            lblError.Visibility = Visibility.Collapsed;
            string documento = txtDocumento.Text;
            string clave = txtClave.Password;

            // 1. Llamamos a la Capa de Negocio
            this.UsuarioLogueado = cn_usuario.Validar(documento, clave);

            // 2. Comprobamos el resultado
            if (this.UsuarioLogueado != null)
            {
                // ¡ÉXITO!
                this.DialogResult = true; // Avisa a App.xaml.cs que fue exitoso
                this.Close();             // Cierra esta ventana
            }
            else
            {
                // ¡FALLO!
                lblError.Visibility = Visibility.Visible;
                // NO hacemos nada más. La ventana permanece abierta
                // y el usuario puede volver a intentarlo.
            }
        }

        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            // El usuario decidió salir
            this.DialogResult = false; // Avisa a App.xaml.cs que el usuario canceló
            this.Close();
        }
    }
}