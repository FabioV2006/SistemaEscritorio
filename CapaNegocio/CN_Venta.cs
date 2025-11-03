using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Venta
    {
        private CD_Venta cd_venta = new CD_Venta();

        public int Registrar(VENTAS obj, DataTable dtDetalle, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.IdCliente == null || obj.IdCliente == 0)
            {
                Mensaje = "Debe seleccionar un Cliente.";
                return 0;
            }
            if (string.IsNullOrEmpty(obj.NumeroDocumento))
            {
                Mensaje = "El Número de Documento no puede estar vacío.";
                return 0;
            }
            if (dtDetalle.Rows.Count == 0)
            {
                Mensaje = "Debe añadir al menos un producto a la venta.";
                return 0;
            }

            return cd_venta.Registrar(obj, dtDetalle, out Mensaje);
        }
        public List<VENTAS> Listar()
        {
            // Aquí podríamos aplicar reglas de negocio para listar,
            // pero por ahora solo llamamos a la capa de datos.
            return cd_venta.Listar();
        }

        public List<DETALLE_VENTAS> ObtenerDetalle(int idVenta)
        {
            if (idVenta == 0)
            {
                return new List<DETALLE_VENTAS>();
            }
            return cd_venta.ObtenerDetalle(idVenta);
        }
    }
}