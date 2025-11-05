using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    // Clase simple para representar los datos del negocio
    public class Negocio
    {
        public string RUC { get; set; }
        public string Direccion { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public byte[] Logo { get; set; }
    }

    public class CN_Negocio
    {
        // Este método debería obtener los datos desde la base de datos
        // Por ahora retorna datos de ejemplo - AJUSTA SEGÚN TU BD
        public Negocio ObtenerDatos()
        {
            // TODO: Implementar lectura desde la base de datos
            // Si tienes una tabla de configuración del negocio, léela aquí
            return new Negocio
            {
                RUC = "20123456789",
                Direccion = "Av. Principal 123, Lima, Perú",
                Correo = "contacto@distribuidora.com",
                Telefono = "(01) 234-5678",
                Logo = null // El logo se maneja por archivo
            };
        }

        public byte[] ObtenerLogo(out bool obtenido)
        {
            // TODO: Si guardas el logo en BD, impleméntalo aquí
            // Por ahora retornamos null y usamos el archivo
            obtenido = false;
            return null;
        }
    }
}