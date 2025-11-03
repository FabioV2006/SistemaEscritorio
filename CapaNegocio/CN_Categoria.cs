using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Categoria
    {
        private CD_Categoria cd_categoria = new CD_Categoria();

        public List<CATEGORIAS> Listar()
        {
            return cd_categoria.Listar();
        }

        public int Registrar(CATEGORIAS obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrEmpty(obj.Descripcion))
            {
                Mensaje = "La descripción no puede ser vacía.";
                return 0;
            }
            return cd_categoria.Registrar(obj, out Mensaje);
        }

        public bool Editar(CATEGORIAS obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrEmpty(obj.Descripcion))
            {
                Mensaje = "La descripción no puede ser vacía.";
                return false;
            }
            return cd_categoria.Editar(obj, out Mensaje);
        }
    }
}