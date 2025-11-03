using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Cliente
    {
        private CD_Cliente cd_cliente = new CD_Cliente();

        public List<CLIENTES> Listar()
        {
            return cd_cliente.Listar();
        }

        public int Registrar(CLIENTES obj, out string Mensaje)
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
            if (string.IsNullOrEmpty(obj.DireccionEnvio))
            {
                Mensaje = "La Dirección de Envío no puede estar vacía.";
                return 0;
            }

            return cd_cliente.Registrar(obj, out Mensaje);
        }

        public bool Editar(CLIENTES obj, out string Mensaje)
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

            return cd_cliente.Editar(obj, out Mensaje);
        }
    }
}