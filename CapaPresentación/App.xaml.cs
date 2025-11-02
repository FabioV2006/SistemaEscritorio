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

            // 1. Crear y mostrar la ventana de Login de forma modal
            LoginWindow loginWindow = new LoginWindow();
            // ShowDialog() la abre y espera a que se cierre
            bool? loginResult = loginWindow.ShowDialog();

            // 2. Comprobar si el login fue exitoso
            //    (Pon tu breakpoint aquí)
            if (loginResult == true)
            {
                // 3. Si fue exitoso, obtener el usuario validado
                USUARIOS usuarioLogueado = loginWindow.UsuarioLogueado;

                // 4. Crear y mostrar la MainWindow, pasándole el usuario
                MainWindow mainWindow = new MainWindow(usuarioLogueado);
                mainWindow.Show();
            }
            else
            {
                // 5. Si loginResult es 'false' o 'null' (el usuario cerró),
                //    la aplicación simplemente termina.
                Application.Current.Shutdown();
            }
        }
    }
}