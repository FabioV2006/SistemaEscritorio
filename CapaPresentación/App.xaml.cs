using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CapaDatos;
namespace CapaPresentación
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Asegúrate de haber BORRADO 'StartupUri="MainWindow.xaml"' de tu App.xaml

            LoginWindow loginWindow = new LoginWindow();
            bool? loginResult = loginWindow.ShowDialog(); // Espera a que se cierre

            if (loginResult == true)
            {
                // El login fue exitoso, obtenemos el usuario
                USUARIOS usuarioLogueado = loginWindow.UsuarioLogueado;

                // Creamos y mostramos la MainWindow, pasándole el usuario
                MainWindow mainWindow = new MainWindow(usuarioLogueado);
                mainWindow.Show();
            }
            else
            {
                // Si loginResult es 'false' o 'null' (el usuario cerró),
                // la aplicación simplemente termina.
                Application.Current.Shutdown();
            }
        }
    }
}