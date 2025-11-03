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
        // Esto busca todos los lotes con stock y no vencidos PARA UN PRODUCTO ESPECÍFICO
        public List<LOTES> ListarLotesPorProducto(int idProducto)
        {
            try
            {
                DateTime fechaActual = DateTime.Now.Date;

                // Traemos los lotes que SÍ podemos vender:
                // 1. Pertenecen al IdProducto específico.
                // 2. Tienen stock (> 0).
                // 3. No están vencidos (FechaVencimiento > Hoy).
                return db.LOTES
                         .Where(l => l.IdProducto == idProducto &&
                                     l.Stock > 0 &&
                                     l.FechaVencimiento > fechaActual)
                         .OrderBy(l => l.FechaVencimiento) // Importante: Muestra primero los que vencen antes
                         .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Lote.ListarLotesPorProducto: " + ex.Message);
                return new List<LOTES>();
            }
        }
    }
}