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
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // IMPORTANTE: Configurar ShutdownMode para que la app NO se cierre automáticamente
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // 1. Crear y mostrar la ventana de Login de forma modal
            LoginWindow loginWindow = new LoginWindow();

            // ShowDialog() bloquea la ejecución hasta que se cierre
            bool? loginResult = loginWindow.ShowDialog();

            // 2. Comprobar si el login fue exitoso
            if (loginResult == true)
            {
                // 3. Login exitoso: obtener el usuario validado
                USUARIOS usuarioLogueado = loginWindow.UsuarioLogueado;

                // 4. Cambiar ShutdownMode para que se cierre cuando se cierre la MainWindow
                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

                // 5. Crear y mostrar la MainWindow con el usuario
                MainWindow mainWindow = new MainWindow(usuarioLogueado);
                Application.Current.MainWindow = mainWindow; // Establecer como ventana principal
                mainWindow.Show();
            }
            else
            {
                // 6. Login cancelado o fallido: cerrar la aplicación
                Application.Current.Shutdown();
            }
        }
    }
}