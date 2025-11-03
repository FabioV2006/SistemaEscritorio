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
    public partial class ModalCategoria : Window
    {
        private CN_Categoria cn_categoria = new CN_Categoria();
        private CATEGORIAS itemActual;
        private bool esNuevo = false;

        public ModalCategoria(CATEGORIAS item = null)
        {
            InitializeComponent();
            if (item == null)
            {
                esNuevo = true;
                txtTitulo.Text = "✚ Nueva Categoría";
                itemActual = new CATEGORIAS();
                chkEstado.IsChecked = true;
            }
            else
            {
                esNuevo = false;
                txtTitulo.Text = "✏️ Editar Categoría";
                itemActual = item;
                txtId.Text = item.IdCategoria.ToString();
                txtDescripcion.Text = item.Descripcion;
                chkEstado.IsChecked = item.Estado;
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = string.Empty;
            itemActual.Descripcion = txtDescripcion.Text;
            itemActual.Estado = chkEstado.IsChecked ?? false;

            if (esNuevo)
            {
                int idGenerado = cn_categoria.Registrar(itemActual, out mensaje);
                if (idGenerado > 0)
                {
                    MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; this.Close();
                }
                else { MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            else
            {
                itemActual.IdCategoria = int.Parse(txtId.Text);
                bool resultado = cn_categoria.Editar(itemActual, out mensaje);
                if (resultado)
                {
                    MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; this.Close();
                }
                else { MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}