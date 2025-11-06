using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Compra
    {
        private CD_Compra cd_compra = new CD_Compra();

        public int Registrar(COMPRAS obj, DataTable dtDetalle, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.IdProveedor == null || obj.IdProveedor == 0)
            {
                Mensaje = "Debe seleccionar un Proveedor.";
                return 0;
            }
            if (string.IsNullOrEmpty(obj.NumeroDocumento))
            {
                Mensaje = "El Número de Documento no puede estar vacío.";
                return 0;
            }
            if (dtDetalle.Rows.Count == 0)
            {
                Mensaje = "Debe añadir al menos un producto/lote a la compra.";
                return 0;
            }

            return cd_compra.Registrar(obj, dtDetalle, out Mensaje);
        }
        public List<COMPRAS> Listar()
        {
            // Aquí podríamos aplicar reglas de negocio para listar,
            // pero por ahora solo llamamos a la capa de datos.
            return cd_compra.Listar();
        }

        public List<DETALLE_COMPRAS> ObtenerDetalle(int idCompra)
        {
            return cd_compra.ObtenerDetalle(idCompra);
        }
        public COMPRAS ObtenerCompraParaDevolucion(string numeroDocumento)
        {
            if (string.IsNullOrEmpty(numeroDocumento))
            {
                return null;
            }
            return cd_compra.ObtenerCompraParaDevolucion(numeroDocumento);
        }
    }
}