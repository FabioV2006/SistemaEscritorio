using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_DevolucionCliente
    {
        private CD_DevolucionCliente cd_devolucion = new CD_DevolucionCliente();

        public int Registrar(DEVOLUCIONES_CLIENTES obj, DataTable dtDetalle, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.IdVenta == null || obj.IdVenta == 0)
            {
                Mensaje = "Debe seleccionar una venta válida.";
                return 0;
            }
            if (string.IsNullOrEmpty(obj.Motivo))
            {
                Mensaje = "El motivo de la devolución no puede estar vacío.";
                return 0;
            }
            if (dtDetalle.Rows.Count == 0)
            {
                Mensaje = "Debe seleccionar al menos un producto para devolver.";
                return 0;
            }

            return cd_devolucion.Registrar(obj, dtDetalle, out Mensaje);
        }
    }
}