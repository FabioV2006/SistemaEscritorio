using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Usuario
    {
        private CD_Usuario cd_usuario = new CD_Usuario();

        public USUARIOS Validar(string documento, string clave)
        {
            // Aquí iría la lógica de negocio, por ejemplo:
            // 1. Validar que los campos no estén vacíos.
            if (string.IsNullOrEmpty(documento) || string.IsNullOrEmpty(clave))
            {
                return null;
            }

            // 2. (Futuro) Desencriptar la clave si estuviera hasheada.

            // 3. Llamar a la capa de datos
            return cd_usuario.Validar(documento, clave);
        }
    }
}
