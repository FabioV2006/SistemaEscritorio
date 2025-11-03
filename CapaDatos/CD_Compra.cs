using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Compra
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        /// <summary>
        /// Registra una Compra completa con sus Lotes y Detalles, todo dentro de una transacción.
        /// </summary>
        /// <param name="obj">El objeto COMPRAS principal</param>
        /// <param name="dtDetalle">Un DataTable que representa la grilla. Columnas esperadas:
        /// IdProducto, NumeroLote, FechaVencimiento, PrecioCompra, PrecioVenta, Cantidad, MontoTotal
        /// </param>
        /// <param name="Mensaje">Mensaje de salida</param>
        /// <returns>El ID de la nueva Compra, o 0 si falla.</returns>
        public int Registrar(COMPRAS obj, DataTable dtDetalle, out string Mensaje)
        {
            Mensaje = string.Empty;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Añadir el objeto Compra principal al contexto
                    db.COMPRAS.Add(obj);

                    // 2. Guardar para obtener el IdCompra (necesario para los detalles, aunque EF podría manejarlo)
                    db.SaveChanges();
                    int idCompraGenerada = obj.IdCompra;

                    // 3. Iterar sobre el DataTable (la grilla)
                    foreach (DataRow row in dtDetalle.Rows)
                    {
                        // 4. Por cada fila, crear un NUEVO LOTE
                        LOTES nuevoLote = new LOTES()
                        {
                            IdProducto = Convert.ToInt32(row["IdProducto"]),
                            NumeroLote = row["NumeroLote"].ToString(),
                            FechaVencimiento = Convert.ToDateTime(row["FechaVencimiento"]),
                            Stock = Convert.ToInt32(row["Cantidad"]), // El stock inicial es la cantidad comprada
                            PrecioCompra = Convert.ToDecimal(row["PrecioCompra"]),
                            PrecioVenta = Convert.ToDecimal(row["PrecioVenta"]),
                            UbicacionAlmacen = "Patio", // Ubicación por defecto al registrar
                            FechaRegistro = DateTime.Now
                        };
                        db.LOTES.Add(nuevoLote);

                        // 5. Guardar el lote AHORA para obtener su ID
                        db.SaveChanges();
                        int idLoteGenerado = nuevoLote.IdLote;

                        // 6. Crear el DETALLE_COMPRAS y enlazarlo
                        DETALLE_COMPRAS detalleCompra = new DETALLE_COMPRAS()
                        {
                            IdCompra = idCompraGenerada,
                            IdLote = idLoteGenerado, // <-- Enlace clave
                            Cantidad = Convert.ToInt32(row["Cantidad"]),
                            PrecioCompraUnitario = Convert.ToDecimal(row["PrecioCompra"]),
                            MontoTotal = Convert.ToDecimal(row["MontoTotal"])
                        };
                        db.DETALLE_COMPRAS.Add(detalleCompra);
                    }

                    // 7. Guardar todos los detalles de compra
                    db.SaveChanges();

                    // 8. Si todo salió bien, confirmar la transacción
                    transaction.Commit();
                    Mensaje = "Compra registrada con éxito.";
                    return idCompraGenerada;
                }
                catch (Exception ex)
                {
                    // 9. Si algo falló, revertir todo
                    transaction.Rollback();
                    Mensaje = "Error al registrar la compra: " + ex.Message;
                    return 0;
                }
            }
        }
        public List<COMPRAS> Listar()
        {
            try
            {
                // Traemos las compras e incluimos la data relacionada del Proveedor y Usuario
                // que mostraremos en la grilla principal.
                return db.COMPRAS
                         .Include("PROVEEDORES") // Usamos strings para EF6
                         .Include("USUARIOS")
                         .OrderByDescending(c => c.FechaRegistro) // Mostrar las más nuevas primero
                         .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Compra.Listar: " + ex.Message);
                return new List<COMPRAS>();
            }
        }

        public List<DETALLE_COMPRAS> ObtenerDetalle(int idCompra)
        {
            try
            {
                // Traemos el detalle de una compra específica.
                // Incluimos el Lote y el Producto de ese lote para mostrar en el modal.
                return db.DETALLE_COMPRAS
                         .Include("LOTES")
                         .Include("LOTES.PRODUCTOS")
                         .Where(dc => dc.IdCompra == idCompra)
                         .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Compra.ObtenerDetalle: " + ex.Message);
                return new List<DETALLE_COMPRAS>();
            }
        }
        public COMPRAS ObtenerCompraParaDevolucion(string numeroDocumento)
        {
            try
            {
                string docBuscado = numeroDocumento.Trim();

                // Carga la compra y toda la info necesaria para la devolución
                return db.COMPRAS
                         .Include("PROVEEDORES")
                         .Include("DETALLE_COMPRAS")
                         .Include("DETALLE_COMPRAS.LOTES") // El detalle de la compra nos da el Lote
                         .Include("DETALLE_COMPRAS.LOTES.PRODUCTOS") // El Lote nos da el Producto
                         .FirstOrDefault(c => string.Equals(c.NumeroDocumento.Trim(), docBuscado, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Compra.ObtenerCompraParaDevolucion: " + ex.Message);
                return null;
            }
        }
    }
}