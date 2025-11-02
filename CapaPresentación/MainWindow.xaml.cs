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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CapaDatos;
using CapaNegocio;

namespace CapaPresentación
{
    public partial class MainWindow : Window
    {
        private USUARIOS _usuarioActual; // Variable para guardar el usuario
        private CN_Permiso cn_permiso = new CN_Permiso(); // Instancia de negocio de permisos

        public MainWindow(USUARIOS usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;

            // 1. Mostrar la info del usuario en la barra superior
            string nombreRol = (_usuarioActual.ROLES != null) ? _usuarioActual.ROLES.Descripcion : "Sin Rol";
            txtUsuarioInfo.Text = $"Bienvenido, {_usuarioActual.NombreCompleto} ({nombreRol})";

            // 2. Cargar permisos y configurar el menú
            CargarPermisos();

            // 3. Cargar la página de dashboard por defecto (si tiene permiso)
            if (BtnDashboard.Visibility == Visibility.Visible)
            {
                MainFrame.Navigate(new Uri("Vistas/DashboardPage.xaml", UriKind.Relative));
                TxtTituloPagina.Text = "Dashboard";
            }
            else
            {
                TxtTituloPagina.Text = "Bienvenido";
                // (Opcional) Navegar a una página de "Bienvenida" simple
            }
        }

        private void CargarPermisos()
        {
            // 1. Obtener la lista de permisos para el rol del usuario
            //    Usamos .Value porque IdRol es nullable, pero en este punto ya debe tener valor
            List<PERMISOS> permisos = cn_permiso.Listar(_usuarioActual.IdRol.Value);

            // 2. Extraer solo los nombres de los menús
            var nombresMenu = permisos.Select(p => p.NombreMenu).ToList();

            // 3. Ocultar TODOS los botones por defecto
            BtnDashboard.Visibility = Visibility.Collapsed;
            BtnRegistrarVenta.Visibility = Visibility.Collapsed;
            BtnHistorialVentas.Visibility = Visibility.Collapsed;
            BtnRegistrarCompra.Visibility = Visibility.Collapsed;
            BtnHistorialCompras.Visibility = Visibility.Collapsed;
            BtnDevClientes.Visibility = Visibility.Collapsed;
            BtnDevProveedores.Visibility = Visibility.Collapsed;
            BtnHistDevClientes.Visibility = Visibility.Collapsed;
            BtnHistDevProv.Visibility = Visibility.Collapsed;
            BtnClientes.Visibility = Visibility.Collapsed;
            BtnProveedores.Visibility = Visibility.Collapsed;
            BtnLotes.Visibility = Visibility.Collapsed;
            BtnReportes.Visibility = Visibility.Collapsed;
            BtnAdministracion.Visibility = Visibility.Collapsed;

            // 4. Mostrar solo los botones que están en la lista de permisos
            //    (Los nombres "Dashboard", "RegistrarVenta", etc. deben coincidir 
            //     exactamente con lo que tienes en tu tabla PERMISOS.NombreMenu)
            if (nombresMenu.Contains("Dashboard")) BtnDashboard.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("RegistrarVenta")) BtnRegistrarVenta.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("HistorialVentas")) BtnHistorialVentas.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("RegistrarCompra")) BtnRegistrarCompra.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("HistorialCompras")) BtnHistorialCompras.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("DevolucionesClientes")) BtnDevClientes.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("DevolucionesProveedor")) BtnDevProveedores.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("HistorialDevClientes")) BtnHistDevClientes.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("HistorialDevProveedor")) BtnHistDevProv.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Clientes")) BtnClientes.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Proveedores")) BtnProveedores.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Inventario")) BtnLotes.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Reportes")) BtnReportes.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Administracion")) BtnAdministracion.Visibility = Visibility.Visible;
        }

        // Un solo manejador de eventos para TODOS los botones del menú
        private void BtnNavigate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                string paginaUrl = btn.Tag.ToString();

                // Navegar el Frame a esa página
                MainFrame.Navigate(new Uri(paginaUrl, UriKind.Relative));

                // Actualizar el título en la barra superior
                TxtTituloPagina.Text = (btn.Content as string).Trim().Substring(2); // Limpia el emoji
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al navegar a la página: {ex.Message}", "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}