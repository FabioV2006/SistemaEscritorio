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
        public List<USUARIOS> Listar()
        {
            return cd_usuario.Listar();
        }

        public int Registrar(USUARIOS obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrEmpty(obj.Documento))
                Mensaje = "El Documento no puede ser vacío.";
            else if (string.IsNullOrEmpty(obj.NombreCompleto))
                Mensaje = "El Nombre Completo no puede ser vacío.";
            else if (string.IsNullOrEmpty(obj.Clave))
                Mensaje = "La Clave no puede ser vacía.";
            else if (obj.IdRol == null || obj.IdRol == 0)
                Mensaje = "Debe seleccionar un Rol.";

            if (!string.IsNullOrEmpty(Mensaje))
                return 0; // 0 indica que hubo un error
            else
                return cd_usuario.Registrar(obj, out Mensaje);
        }

        public bool Editar(USUARIOS obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrEmpty(obj.Documento))
                Mensaje = "El Documento no puede ser vacío.";
            else if (string.IsNullOrEmpty(obj.NombreCompleto))
                Mensaje = "El Nombre Completo no puede ser vacío.";
            else if (string.IsNullOrEmpty(obj.Clave))
                Mensaje = "La Clave no puede ser vacía.";
            else if (obj.IdRol == null || obj.IdRol == 0)
                Mensaje = "Debe seleccionar un Rol.";

            if (!string.IsNullOrEmpty(Mensaje))
                return false;
            else
                return cd_usuario.Editar(obj, out Mensaje);
        }
    }
}
