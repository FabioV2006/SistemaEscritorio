using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Rol
    {
        private CD_Rol cd_rol = new CD_Rol();

        public List<ROLES> Listar()
        {
            return cd_rol.Listar();
        }

        // El parámetro 'permisos' es la lista de objetos PERMISO
        // que vienen de la UI (con NombreMenu)
        public int Registrar(ROLES obj, List<PERMISOS> permisos, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrEmpty(obj.Descripcion))
            {
                Mensaje = "La descripción del rol no puede ser vacía.";
                return 0;
            }
            if (permisos.Count == 0)
            {
                Mensaje = "Debe asignar al menos un permiso al rol.";
                return 0;
            }

            return cd_rol.Registrar(obj, permisos, out Mensaje);
        }

        public bool Editar(ROLES obj, List<PERMISOS> permisos, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrEmpty(obj.Descripcion))
            {
                Mensaje = "La descripción del rol no puede ser vacía.";
                return false;
            }
            if (permisos.Count == 0)
            {
                Mensaje = "Debe asignar al menos un permiso al rol.";
                return false;
            }

            return cd_rol.Editar(obj, permisos, out Mensaje);
        }
    }
}