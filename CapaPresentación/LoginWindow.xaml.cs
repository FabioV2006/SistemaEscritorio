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
        // 2. Propiedad pública para pasar el usuario validado
        public USUARIOS UsuarioLogueado { get; private set; }

        private CN_Usuario cn_usuario = new CN_Usuario();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnIngresar_Click(object sender, RoutedEventArgs e)
        {
            lblError.Visibility = Visibility.Collapsed; // Oculta error

            string documento = txtDocumento.Text;
            string clave = txtClave.Password; // Se usa .Password para PasswordBox

            // 3. Llamar a la Capa de Negocio para validar
            this.UsuarioLogueado = cn_usuario.Validar(documento, clave);

            // 4. Comprobar el resultado
            if (this.UsuarioLogueado != null)
            {
                // ¡Éxito!
                this.DialogResult = true; // Indicar éxito a App.xaml.cs
                this.Close();             // Cerrar esta ventana
            }
            else
            {
                // Error
                lblError.Visibility = Visibility.Visible;
                this.DialogResult = false;
            }
        }

        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}