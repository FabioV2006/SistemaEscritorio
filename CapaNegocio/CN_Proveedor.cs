using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Proveedor
    {
        private CD_Proveedor cd_proveedor = new CD_Proveedor();

        public List<PROVEEDORES> Listar()
        {
            return cd_proveedor.Listar();
        }

        public int Registrar(PROVEEDORES obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrEmpty(obj.Documento))
            {
                Mensaje = "El Documento (RUC) no puede estar vacío.";
                return 0;
            }
            if (string.IsNullOrEmpty(obj.RazonSocial))
            {
                Mensaje = "La Razón Social no puede estar vacía.";
                return 0;
            }

            return cd_proveedor.Registrar(obj, out Mensaje);
        }

        public bool Editar(PROVEEDORES obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrEmpty(obj.Documento))
            {
                Mensaje = "El Documento (RUC) no puede estar vacío.";
                return false;
            }
            if (string.IsNullOrEmpty(obj.RazonSocial))
            {
                Mensaje = "La Razón Social no puede estar vacía.";
                return false;
            }

            return cd_proveedor.Editar(obj, out Mensaje);
        }
    }
}