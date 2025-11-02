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

            // 1. Quita "StartupUri=" de tu App.xaml

            // 2. Crear y mostrar la ventana de Login de forma modal
            LoginWindow loginWindow = new LoginWindow();
            bool? loginResult = loginWindow.ShowDialog();

            // 3. Comprobar si el login fue exitoso
            if (loginResult == true)
            {
                // 4. Si fue exitoso, obtener el usuario validado
                USUARIOS usuarioLogueado = loginWindow.UsuarioLogueado;

                // 5. Crear y mostrar la MainWindow, pasándole el usuario
                MainWindow mainWindow = new MainWindow(usuarioLogueado);
                mainWindow.Show();
            }
            else
            {
                // 6. Si el login se cierra o falla, la aplicación termina.
                Application.Current.Shutdown();
            }
        }
    }
}