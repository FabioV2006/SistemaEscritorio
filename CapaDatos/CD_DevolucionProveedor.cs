using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_DevolucionProveedor
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        /// <summary>
        /// Registra una Devolución a Proveedor y DESCUENTA el stock del lote original.
        /// </summary>
        /// <param name="obj">El objeto DEVOLUCIONES_PROVEEDORES principal</param>
        /// <param name="dtDetalle">Columnas: IdLote, Cantidad, MontoUnitario (costo), SubTotal</param>
        /// <param name="Mensaje">Mensaje de salida</param>
        /// <returns>El ID de la nueva devolución, o 0 si falla.</returns>
        public int Registrar(DEVOLUCIONES_PROVEEDORES obj, DataTable dtDetalle, out string Mensaje)
        {
            Mensaje = string.Empty;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Añadir el objeto Devolucion principal
                    db.DEVOLUCIONES_PROVEEDORES.Add(obj);
                    db.SaveChanges(); // Guardamos para obtener el IdDevolucionProveedor

                    int idDevolucionGenerada = obj.IdDevolucionProveedor;

                    // 2. Iterar sobre el DataTable (la grilla de la UI)
                    foreach (DataRow row in dtDetalle.Rows)
                    {
                        int idLote = Convert.ToInt32(row["IdLote"]);
                        int cantidadDevuelta = Convert.ToInt32(row["Cantidad"]);

                        // 3. Buscar el lote en la BD
                        LOTES lote = db.LOTES.Find(idLote);
                        if (lote == null)
                        {
                            throw new Exception($"El lote con ID {idLote} no fue encontrado.");
                        }

                        // 4. VERIFICACIÓN DE STOCK (CRÍTICO)
                        if (lote.Stock < cantidadDevuelta)
                        {
                            throw new Exception($"Stock insuficiente para devolver del lote {lote.NumeroLote}. Stock actual: {lote.Stock}");
                        }

                        // 5. DESCONTAR EL STOCK
                        lote.Stock -= cantidadDevuelta;

                        // 6. Crear el Detalle de Devolución
                        DETALLE_DEVOLUCIONES_PROVEEDORES detalleDevolucion = new DETALLE_DEVOLUCIONES_PROVEEDORES()
                        {
                            IdDevolucionProveedor = idDevolucionGenerada,
                            IdLote = idLote,
                            Cantidad = cantidadDevuelta,
                            MontoUnitario = Convert.ToDecimal(row["MontoUnitario"]), // Precio de compra
                            SubTotal = Convert.ToDecimal(row["SubTotal"])
                        };
                        db.DETALLE_DEVOLUCIONES_PROVEEDORES.Add(detalleDevolucion);
                    }

                    // 7. Guardar todos los cambios (Stock actualizado y Detalles insertados)
                    db.SaveChanges();

                    // 8. Si todo salió bien, confirmar la transacción
                    transaction.Commit();
                    Mensaje = "Devolución a proveedor registrada con éxito. El stock ha sido descontado.";
                    return idDevolucionGenerada;
                }
                catch (Exception ex)
                {
                    // 9. Si algo falló (ej. stock insuficiente), revertir todo
                    transaction.Rollback();
                    Mensaje = "Error al registrar la devolución: " + ex.Message;
                    return 0;
                }
            }
        }

        // --- MÉTODOS PARA EL HISTORIAL ---

        public List<DEVOLUCIONES_PROVEEDORES> Listar()
        {
            try
            {
                return db.DEVOLUCIONES_PROVEEDORES
                         .Include("COMPRAS")
                         .Include("COMPRAS.PROVEEDORES")
                         .Include("USUARIOS")
                         .OrderByDescending(d => d.FechaRegistro)
                         .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_DevolucionProveedor.Listar: " + ex.Message);
                return new List<DEVOLUCIONES_PROVEEDORES>();
            }
        }

        public List<DETALLE_DEVOLUCIONES_PROVEEDORES> ObtenerDetalle(int idDevolucionProveedor)
        {
            try
            {
                return db.DETALLE_DEVOLUCIONES_PROVEEDORES
                         .Include("LOTES")
                         .Include("LOTES.PRODUCTOS")
                         .Where(dd => dd.IdDevolucionProveedor == idDevolucionProveedor)
                         .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_DevolucionProveedor.ObtenerDetalle: " + ex.Message);
                return new List<DETALLE_DEVOLUCIONES_PROVEEDORES>();
            }
        }
    }
}