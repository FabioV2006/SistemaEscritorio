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

namespace CapaPresentación.Vistas
{
    public partial class AdministracionPage : Page
    {
        // Propiedades de Negocio
        private CN_Usuario cn_usuario = new CN_Usuario();
        private CN_Rol cn_rol = new CN_Rol();
        private CN_Categoria cn_categoria = new CN_Categoria();
        private CN_Laboratorio cn_laboratorio = new CN_Laboratorio();

        // Listas de datos
        private List<USUARIOS> listaUsuarios;
        private List<ROLES> listaRoles;
        private List<CATEGORIAS> listaCategorias;
        private List<LABORATORIOS> listaLaboratorios;

        public AdministracionPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // --- CORRECCIÓN ---
            // Cargamos SOLO la primera pestaña (Usuarios) por defecto.
            // Las otras se cargarán cuando el usuario haga clic en ellas.
            CargarUsuarios();
        }

        // --- MANEJADOR PRINCIPAL DE PESTAÑAS (LA CORRECCIÓN) ---
        // Este evento se dispara CADA VEZ que el usuario cambia de pestaña
        private void AdminTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Verificamos que el evento no sea nulo y que sea del TabControl
            if (e.Source is TabControl)
            {
                TabItem tabSeleccionada = (TabItem)AdminTabControl.SelectedItem;

                // Cargamos los datos de la pestaña seleccionada
                if (tabSeleccionada == TabRoles)
                {
                    CargarRoles();
                }
                else if (tabSeleccionada == TabCategorias)
                {
                    CargarCategorias();
                }
                else if (tabSeleccionada == TabLaboratorios)
                {
                    CargarLaboratorios();
                }
                // Si es TabUsuarios, no hacemos nada extra porque ya se carga al inicio.
            }
        }

        // --- LÓGICA DE TAB USUARIOS ---
        private void CargarUsuarios()
        {
            listaUsuarios = cn_usuario.Listar();
            dgUsuarios.ItemsSource = listaUsuarios;
        }

        private void btnNuevoUsuario_Click(object sender, RoutedEventArgs e)
        {
            ModalUsuario modal = new ModalUsuario(null);
            bool? resultado = modal.ShowDialog();
            if (resultado == true)
            {
                CargarUsuarios(); // Recargar la grilla
            }
        }

        private void btnEditarUsuario_Click(object sender, RoutedEventArgs e)
        {
            USUARIOS usuarioSeleccionado = (sender as Button).DataContext as USUARIOS;
            if (usuarioSeleccionado == null) return;

            ModalUsuario modal = new ModalUsuario(usuarioSeleccionado);
            bool? resultado = modal.ShowDialog();
            if (resultado == true)
            {
                CargarUsuarios();
            }
        }

        private void btnEstadoUsuario_Click(object sender, RoutedEventArgs e)
        {
            USUARIOS usuarioSeleccionado = (sender as Button).DataContext as USUARIOS;
            if (usuarioSeleccionado == null) return;

            string mensaje;
            string accion = (usuarioSeleccionado.Estado ?? false) ? "Desactivar" : "Activar";

            if (MessageBox.Show($"¿Está seguro de {accion} al usuario: {usuarioSeleccionado.NombreCompleto}?",
                "Confirmar Acción", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                usuarioSeleccionado.Estado = !(usuarioSeleccionado.Estado ?? false);
                bool resultado = cn_usuario.Editar(usuarioSeleccionado, out mensaje);
                if (resultado)
                {
                    MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarUsuarios();
                }
                else
                {
                    MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    usuarioSeleccionado.Estado = !(usuarioSeleccionado.Estado ?? false); // Revertir si falla
                }
            }
        }

        // --- LÓGICA DE TAB ROLES ---
        private void CargarRoles()
        {
            listaRoles = cn_rol.Listar();
            dgRoles.ItemsSource = listaRoles;
        }

        private void btnNuevoRol_Click(object sender, RoutedEventArgs e)
        {
            ModalRol modal = new ModalRol(null);
            bool? resultado = modal.ShowDialog();
            if (resultado == true)
            {
                CargarRoles();
            }
        }

        private void btnEditarRol_Click(object sender, RoutedEventArgs e)
        {
            ROLES rolSeleccionado = (sender as Button).DataContext as ROLES;
            if (rolSeleccionado == null) return;

            ModalRol modal = new ModalRol(rolSeleccionado);
            bool? resultado = modal.ShowDialog();
            if (resultado == true)
            {
                CargarRoles();
            }
        }

        // --- LÓGICA DE TAB CATEGORÍAS ---
        private void CargarCategorias()
        {
            listaCategorias = cn_categoria.Listar();
            dgCategorias.ItemsSource = listaCategorias;
        }

        private void btnNuevaCategoria_Click(object sender, RoutedEventArgs e)
        {
            ModalCategoria modal = new ModalCategoria(null);
            bool? resultado = modal.ShowDialog();
            if (resultado == true)
            {
                CargarCategorias();
            }
        }

        private void btnEditarCategoria_Click(object sender, RoutedEventArgs e)
        {
            CATEGORIAS item = (sender as Button).DataContext as CATEGORIAS;
            if (item == null) return;

            ModalCategoria modal = new ModalCategoria(item);
            bool? resultado = modal.ShowDialog();
            if (resultado == true)
            {
                CargarCategorias();
            }
        }

        // --- LÓGICA DE TAB LABORATORIOS ---
        private void CargarLaboratorios()
        {
            listaLaboratorios = cn_laboratorio.Listar();
            dgLaboratorios.ItemsSource = listaLaboratorios;
        }

        private void btnNuevoLaboratorio_Click(object sender, RoutedEventArgs e)
        {
            ModalLaboratorio modal = new ModalLaboratorio(null);
            bool? resultado = modal.ShowDialog();
            if (resultado == true)
            {
                CargarLaboratorios();
            }
        }

        private void btnEditarLaboratorio_Click(object sender, RoutedEventArgs e)
        {
            LABORATORIOS item = (sender as Button).DataContext as LABORATORIOS;
            if (item == null) return;

            ModalLaboratorio modal = new ModalLaboratorio(item);
            bool? resultado = modal.ShowDialog();
            if (resultado == true)
            {
                CargarLaboratorios();
            }
        }
    }
}