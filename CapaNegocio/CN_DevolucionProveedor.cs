using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_DevolucionProveedor
    {
        private CD_DevolucionProveedor cd_devolucion = new CD_DevolucionProveedor();

        public int Registrar(DEVOLUCIONES_PROVEEDORES obj, DataTable dtDetalle, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.IdCompra == null || obj.IdCompra == 0)
            {
                Mensaje = "Debe seleccionar una compra válida.";
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

        public List<DEVOLUCIONES_PROVEEDORES> Listar()
        {
            return cd_devolucion.Listar();
        }

        public List<DETALLE_DEVOLUCIONES_PROVEEDORES> ObtenerDetalle(int idDevolucionProveedor)
        {
            if (idDevolucionProveedor == 0)
            {
                return new List<DETALLE_DEVOLUCIONES_PROVEEDORES>();
            }
            return cd_devolucion.ObtenerDetalle(idDevolucionProveedor);
        }
    }
}