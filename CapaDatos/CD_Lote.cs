using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Lote
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        public List<LOTES> Listar()
        {
            try
            {
                // CORRECCIÓN: Usamos strings para "Include" en EF6
                return db.LOTES
                         .Include("PRODUCTOS")
                         .Include("PRODUCTOS.CATEGORIAS")
                         .Include("PRODUCTOS.LABORATORIOS")
                         .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<LOTES>();
            }
        }

        public bool Editar(LOTES obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            try
            {
                var loteExistente = db.LOTES.Find(obj.IdLote);
                if (loteExistente == null)
                {
                    Mensaje = "Lote no encontrado.";
                    return false;
                }

                // Campos que permitimos editar desde esta pantalla (Ajuste de Inventario)
                loteExistente.Stock = obj.Stock;
                loteExistente.PrecioVenta = obj.PrecioVenta;
                loteExistente.UbicacionAlmacen = obj.UbicacionAlmacen;

                db.SaveChanges();
                Mensaje = "Lote actualizado con éxito.";
                return true;
            }
            catch (Exception ex)
            {
                Mensaje = "Error al editar el lote: " + ex.Message;
                return false;
            }
        }
    }
}