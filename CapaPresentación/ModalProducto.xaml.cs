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
    public partial class ModalProducto : Window
    {
        private CN_Producto cn_producto = new CN_Producto();
        private CN_Categoria cn_categoria = new CN_Categoria();
        private CN_Laboratorio cn_laboratorio = new CN_Laboratorio();

        private PRODUCTOS productoActual;
        private bool esNuevo = false;

        public ModalProducto(PRODUCTOS producto = null)
        {
            InitializeComponent();

            if (producto == null)
            {
                esNuevo = true;
                txtTitulo.Text = "✚ Nuevo Producto";
                productoActual = new PRODUCTOS();
                chkEstado.IsChecked = true;
                chkRequiereReceta.IsChecked = false;
            }
            else
            {
                esNuevo = false;
                txtTitulo.Text = "✏️ Editar Producto";
                productoActual = producto;

                // Cargar datos en los campos
                txtIdProducto.Text = producto.IdProducto.ToString();
                txtCodigo.Text = producto.Codigo;
                txtNombre.Text = producto.Nombre;
                txtDescripcion.Text = producto.Descripcion;
                chkRequiereReceta.IsChecked = producto.RequiereReceta;
                chkEstado.IsChecked = producto.Estado;

                // Los ComboBoxes se seleccionarán en el evento Loaded
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarComboBoxes();
        }

        private void CargarComboBoxes()
        {
            // Cargar Categorías
            var categorias = cn_categoria.Listar();
            cboCategoria.ItemsSource = categorias;
            cboCategoria.DisplayMemberPath = "Descripcion";
            cboCategoria.SelectedValuePath = "IdCategoria";

            // Cargar Laboratorios
            var laboratorios = cn_laboratorio.Listar();
            cboLaboratorio.ItemsSource = laboratorios;
            cboLaboratorio.DisplayMemberPath = "Nombre";
            cboLaboratorio.SelectedValuePath = "IdLaboratorio";

            // Si estamos editando, seleccionar los valores correctos
            if (!esNuevo)
            {
                cboCategoria.SelectedValue = productoActual.IdCategoria;
                cboLaboratorio.SelectedValue = productoActual.IdLaboratorio;
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = string.Empty;

            // 1. Llenar el objeto productoActual
            productoActual.Codigo = txtCodigo.Text;
            productoActual.Nombre = txtNombre.Text;
            productoActual.Descripcion = txtDescripcion.Text;

            // Validar y asignar ComboBoxes
            if (cboCategoria.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar una Categoría.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            productoActual.IdCategoria = (int)cboCategoria.SelectedValue;

            if (cboLaboratorio.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar un Laboratorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            productoActual.IdLaboratorio = (int)cboLaboratorio.SelectedValue;

            productoActual.RequiereReceta = chkRequiereReceta.IsChecked ?? false;
            productoActual.Estado = chkEstado.IsChecked ?? false;

            if (esNuevo)
            {
                // ---- MODO REGISTRAR ----
                int idGenerado = cn_producto.Registrar(productoActual, out mensaje);

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
                productoActual.IdProducto = int.Parse(txtIdProducto.Text);
                bool resultado = cn_producto.Editar(productoActual, out mensaje);

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