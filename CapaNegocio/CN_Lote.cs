using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Lote
    {
        private CD_Lote cd_lote = new CD_Lote();

        public List<LOTES> Listar()
        {
            return cd_lote.Listar();
        }

        public bool Editar(LOTES obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.Stock < 0)
            {
                Mensaje = "El Stock no puede ser un número negativo.";
                return false;
            }
            if (obj.PrecioVenta <= 0)
            {
                Mensaje = "El Precio de Venta debe ser mayor a cero.";
                return false;
            }
            if (string.IsNullOrEmpty(obj.UbicacionAlmacen))
            {
                Mensaje = "La ubicación no puede estar vacía.";
                return false;
            }

            return cd_lote.Editar(obj, out Mensaje);
        }
    }
}