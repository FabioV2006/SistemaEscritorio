using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Producto
    {
        private CD_Producto cd_producto = new CD_Producto();

        public List<PRODUCTOS> Listar()
        {
            return cd_producto.Listar();
        }

        public int Registrar(PRODUCTOS obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrEmpty(obj.Codigo))
            {
                Mensaje = "El Código del producto no puede ser vacío.";
                return 0;
            }
            if (string.IsNullOrEmpty(obj.Nombre))
            {
                Mensaje = "El Nombre del producto no puede ser vacío.";
                return 0;
            }
            if (obj.IdCategoria == null || obj.IdCategoria == 0)
            {
                Mensaje = "Debe seleccionar una Categoría.";
                return 0;
            }
            if (obj.IdLaboratorio == null || obj.IdLaboratorio == 0)
            {
                Mensaje = "Debe seleccionar un Laboratorio.";
                return 0;
            }

            return cd_producto.Registrar(obj, out Mensaje);
        }

        public bool Editar(PRODUCTOS obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrEmpty(obj.Codigo))
            {
                Mensaje = "El Código del producto no puede ser vacío.";
                return false;
            }
            if (string.IsNullOrEmpty(obj.Nombre))
            {
                Mensaje = "El Nombre del producto no puede ser vacío.";
                return false;
            }
            if (obj.IdCategoria == null || obj.IdCategoria == 0)
            {
                Mensaje = "Debe seleccionar una Categoría.";
                return false;
            }
            if (obj.IdLaboratorio == null || obj.IdLaboratorio == 0)
            {
                Mensaje = "Debe seleccionar un Laboratorio.";
                return false;
            }

            return cd_producto.Editar(obj, out Mensaje);
        }
    }
}