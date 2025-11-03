using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Permiso
    {
        private CD_Permiso cd_permiso = new CD_Permiso();

        public List<PERMISOS> Listar(int idRol)
        {
            if (idRol == 0) // Validar que el IdRol sea válido
            {
                return new List<PERMISOS>();
            }

            return cd_permiso.Listar(idRol);
        }
    }
}