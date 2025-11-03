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
        private USUARIOS _usuarioActual;
        private CN_Permiso cn_permiso = new CN_Permiso();
        private List<PERMISOS> _permisosUsuario;

        // 1. El constructor ACEPTA el usuario que viene del Login
        public MainWindow(USUARIOS usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;

            // 2. Mostrar la info del usuario en la barra superior
            string nombreRol = (_usuarioActual.ROLES != null) ? _usuarioActual.ROLES.Descripcion : "Sin Rol";
            txtUsuarioInfo.Text = $"Bienvenido, {_usuarioActual.NombreCompleto} ({nombreRol})";

            // 3. Cargar permisos y configurar el menú
            CargarPermisos();

            // 4. Cargar la página de dashboard por defecto (si tiene permiso)
            if (BtnDashboard.Visibility == Visibility.Visible)
            {
                // Asegúrate de crear una Page "Vistas/DashboardPage.xaml"
                MainFrame.Navigate(new Uri("Vistas/DashboardPage.xaml", UriKind.Relative));
                TxtTituloPagina.Text = "Dashboard";
            }
            else
            {
                TxtTituloPagina.Text = "Bienvenido";
            }
        }

        // ESTA ES LA LÓGICA DE PERMISOS BASADA EN EL ROL
        private void CargarPermisos()
        {
            // 1. Obtener la lista de permisos para el rol del usuario desde la CapaNegocio
            _permisosUsuario = cn_permiso.Listar(_usuarioActual.IdRol.Value);

            // 2. Extraer solo los nombres de los menús
            var nombresMenu = _permisosUsuario.Select(p => p.NombreMenu).ToList();

            // 3. Ocultar TODOS los botones por defecto
            //    Usamos el x:Name del StackPanel para iterar sus hijos
            foreach (var child in MenuStackPanel.Children)
            {
                if (child is Button button) // Solo afecta a los Botones
                {
                    button.Visibility = Visibility.Collapsed;
                }
            }

            // 4. Mostrar solo los botones que están en la lista de permisos
            //    (Los nombres deben coincidir EXACTAMENTE con tu script SQL de datos de prueba)
            if (nombresMenu.Contains("Dashboard")) BtnDashboard.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Registrar Venta")) BtnRegistrarVenta.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Historial de Ventas")) BtnHistorialVentas.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Registrar Compra")) BtnRegistrarCompra.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Historial de Compras")) BtnHistorialCompras.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Dev. de Clientes")) BtnDevClientes.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Dev. a Proveedor")) BtnDevProveedores.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Historial Dev. Clientes")) BtnHistDevClientes.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Historial Dev. Proveedor")) BtnHistDevProv.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Clientes")) BtnClientes.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Proveedores")) BtnProveedores.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Inventario (Lotes)")) BtnLotes.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Reportes")) BtnReportes.Visibility = Visibility.Visible;
            if (nombresMenu.Contains("Administración")) BtnAdministracion.Visibility = Visibility.Visible;
        }

        // Un solo manejador de eventos para TODOS los botones del menú
        private void BtnNavigate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                string paginaUrl = btn.Tag.ToString();

                MainFrame.Navigate(new Uri(paginaUrl, UriKind.Relative));

                // Actualiza el título (limpiando el emoji)
                TxtTituloPagina.Text = (btn.Content as string).Substring((btn.Content as string).IndexOf(" ") + 1);
            }
            catch (Exception ex)
            {
                // Esto pasará si no has creado la Page (ej. "Vistas/DashboardPage.xaml")
                MessageBox.Show($"Error al navegar: {ex.Message}\n\n¿Creaste el archivo {((Button)sender).Tag.ToString()}?", "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            // Cambiar ShutdownMode temporalmente
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Cerrar esta ventana
            this.Close();

            // Mostrar el login nuevamente
            LoginWindow login = new LoginWindow();
            bool? result = login.ShowDialog();

            if (result == true)
            {
                // Si el login es exitoso, abrir una nueva MainWindow
                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                MainWindow mainWindow = new MainWindow(login.UsuarioLogueado);
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
            }
            else
            {
                // Si cancela el login, cerrar la aplicación
                Application.Current.Shutdown();
            }
        }
    }
}