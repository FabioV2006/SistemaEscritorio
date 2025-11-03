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
    // Clase interna (ViewModel) para manejar la lista de CheckBoxes
    public class PermisoViewModel
    {
        public string Nombre { get; set; }
        public bool Asignado { get; set; }
    }

    public partial class ModalRol : Window
    {
        private CN_Rol cn_rol = new CN_Rol();
        private CN_Permiso cn_permiso = new CN_Permiso();

        private ROLES rolActual;
        private bool esNuevo = false;

        // Esta es la lista MAESTRA de todos los permisos que existen en tu app
        // Debe coincidir con los "NombreMenu" de tu script SQL y de MainWindow.xaml.cs
        private List<string> todosLosPermisos = new List<string> {
            "Dashboard",
            "Registrar Venta",
            "Historial de Ventas",
            "Registrar Compra",
            "Historial de Compras",
            "Dev. de Clientes",
            "Dev. a Proveedor",
            "Historial Dev. Clientes",
            "Historial Dev. Proveedor",
            "Clientes",
            "Proveedores",
            "Inventario (Lotes)",
            "Productos", // El que acabamos de agregar
            "Reportes",
            "Administración"
        };

        private List<PermisoViewModel> listaPermisosVM = new List<PermisoViewModel>();

        public ModalRol(ROLES rol = null)
        {
            InitializeComponent();

            if (rol == null)
            {
                esNuevo = true;
                txtTitulo.Text = "✚ Nuevo Rol";
                rolActual = new ROLES();
            }
            else
            {
                esNuevo = false;
                txtTitulo.Text = "✏️ Editar Rol y Permisos";
                rolActual = rol;

                txtIdRol.Text = rol.IdRol.ToString();
                txtDescripcion.Text = rol.Descripcion;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarCheckboxesPermisos();
        }

        private void CargarCheckboxesPermisos()
        {
            List<PERMISOS> permisosActuales = new List<PERMISOS>();

            // Si estamos editando, cargamos los permisos que el rol YA tiene
            if (!esNuevo)
            {
                permisosActuales = cn_permiso.Listar(rolActual.IdRol);
            }

            // Creamos la lista de ViewModels
            foreach (string nombrePermiso in todosLosPermisos)
            {
                listaPermisosVM.Add(new PermisoViewModel
                {
                    Nombre = nombrePermiso,
                    // Comprobamos si el rol actual tiene este permiso
                    Asignado = permisosActuales.Any(p => p.NombreMenu == nombrePermiso)
                });
            }

            // Asignamos la lista al ItemsControl (la lista de CheckBoxes)
            icPermisos.ItemsSource = listaPermisosVM;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = string.Empty;

            // 1. Llenar el objeto Rol
            rolActual.Descripcion = txtDescripcion.Text;

            // 2. Crear la lista de permisos seleccionados
            List<PERMISOS> permisosSeleccionados = new List<PERMISOS>();
            foreach (PermisoViewModel item in icPermisos.Items)
            {
                if (item.Asignado)
                {
                    permisosSeleccionados.Add(new PERMISOS
                    {
                        // IdRol se asignará en la Capa de Datos
                        NombreMenu = item.Nombre
                    });
                }
            }

            if (esNuevo)
            {
                // ---- MODO REGISTRAR ----
                int idGenerado = cn_rol.Registrar(rolActual, permisosSeleccionados, out mensaje);
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
                rolActual.IdRol = int.Parse(txtIdRol.Text);
                bool resultado = cn_rol.Editar(rolActual, permisosSeleccionados, out mensaje);
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