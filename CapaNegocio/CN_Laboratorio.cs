using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Laboratorio
    {
        private CD_Laboratorio cd_laboratorio = new CD_Laboratorio();

        public List<LABORATORIOS> Listar()
        {
            return cd_laboratorio.Listar();
        }

        public int Registrar(LABORATORIOS obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrEmpty(obj.Nombre))
            {
                Mensaje = "El nombre no puede ser vacío.";
                return 0;
            }
            return cd_laboratorio.Registrar(obj, out Mensaje);
        }

        public bool Editar(LABORATORIOS obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrEmpty(obj.Nombre))
            {
                Mensaje = "El nombre no puede ser vacío.";
                return false;
            }
            return cd_laboratorio.Editar(obj, out Mensaje);
        }
    }
}