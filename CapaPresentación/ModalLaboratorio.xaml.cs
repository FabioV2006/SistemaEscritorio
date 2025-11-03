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
    public partial class ModalLaboratorio : Window
    {
        private CN_Laboratorio cn_laboratorio = new CN_Laboratorio();
        private LABORATORIOS itemActual;
        private bool esNuevo = false;

        public ModalLaboratorio(LABORATORIOS item = null)
        {
            InitializeComponent();
            if (item == null)
            {
                esNuevo = true;
                txtTitulo.Text = "✚ Nuevo Laboratorio";
                itemActual = new LABORATORIOS();
                chkEstado.IsChecked = true;
            }
            else
            {
                esNuevo = false;
                txtTitulo.Text = "✏️ Editar Laboratorio";
                itemActual = item;
                txtId.Text = item.IdLaboratorio.ToString();
                txtNombre.Text = item.Nombre;
                txtDescripcion.Text = item.Descripcion;
                chkEstado.IsChecked = item.Estado;
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = string.Empty;
            itemActual.Nombre = txtNombre.Text;
            itemActual.Descripcion = txtDescripcion.Text;
            itemActual.Estado = chkEstado.IsChecked ?? false;

            if (esNuevo)
            {
                int idGenerado = cn_laboratorio.Registrar(itemActual, out mensaje);
                if (idGenerado > 0)
                {
                    MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; this.Close();
                }
                else { MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            else
            {
                itemActual.IdLaboratorio = int.Parse(txtId.Text);
                bool resultado = cn_laboratorio.Editar(itemActual, out mensaje);
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