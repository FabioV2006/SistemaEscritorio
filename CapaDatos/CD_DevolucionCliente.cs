using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_DevolucionCliente
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        /// <summary>
        /// Registra una Devolución de Cliente y restaura el stock al lote original.
        /// </summary>
        /// <param name="obj">El objeto DEVOLUCIONES_CLIENTES principal</param>
        /// <param name="dtDetalle">Columnas: IdLote, Cantidad, MontoUnitario, SubTotal</param>
        /// <param name="Mensaje">Mensaje de salida</param>
        /// <returns>El ID de la nueva devolución, o 0 si falla.</returns>
        public int Registrar(DEVOLUCIONES_CLIENTES obj, DataTable dtDetalle, out string Mensaje)
        {
            Mensaje = string.Empty;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Añadir el objeto Devolucion principal
                    db.DEVOLUCIONES_CLIENTES.Add(obj);
                    db.SaveChanges(); // Guardamos para obtener el IdDevolucionCliente

                    int idDevolucionGenerada = obj.IdDevolucionCliente;

                    // 2. Iterar sobre el DataTable (la grilla de la UI)
                    foreach (DataRow row in dtDetalle.Rows)
                    {
                        int idLote = Convert.ToInt32(row["IdLote"]);
                        int cantidadDevuelta = Convert.ToInt32(row["Cantidad"]);

                        // 3. Buscar el lote en la BD
                        LOTES lote = db.LOTES.Find(idLote);
                        if (lote == null)
                        {
                            throw new Exception($"El lote con ID {idLote} no fue encontrado. No se puede devolver stock.");
                        }

                        // 4. RESTAURAR EL STOCK (CRÍTICO)
                        lote.Stock += cantidadDevuelta;

                        // 5. Crear el Detalle de Devolución
                        DETALLE_DEVOLUCIONES_CLIENTES detalleDevolucion = new DETALLE_DEVOLUCIONES_CLIENTES()
                        {
                            IdDevolucionCliente = idDevolucionGenerada,
                            IdLote = idLote,
                            Cantidad = cantidadDevuelta,
                            MontoUnitario = Convert.ToDecimal(row["MontoUnitario"]),
                            SubTotal = Convert.ToDecimal(row["SubTotal"])
                        };
                        db.DETALLE_DEVOLUCIONES_CLIENTES.Add(detalleDevolucion);
                    }

                    // 7. Guardar todos los cambios (Stock actualizado y Detalles insertados)
                    db.SaveChanges();

                    // 8. Si todo salió bien, confirmar la transacción
                    transaction.Commit();
                    Mensaje = "Devolución registrada con éxito. El stock ha sido restaurado.";
                    return idDevolucionGenerada;
                }
                catch (Exception ex)
                {
                    // 9. Si algo falló, revertir todo
                    transaction.Rollback();
                    Mensaje = "Error al registrar la devolución: " + ex.Message;
                    return 0;
                }
            }
        }

        public List<DEVOLUCIONES_CLIENTES> Listar()
        {
            try
            {
                // Traemos las devoluciones e incluimos la data relacionada
                // Devolucion -> Venta -> Cliente
                // Devolucion -> Usuario
                return db.DEVOLUCIONES_CLIENTES
                         .Include("VENTAS")
                         .Include("VENTAS.CLIENTES")
                         .Include("USUARIOS")
                         .OrderByDescending(d => d.FechaRegistro) // Mostrar las más nuevas primero
                         .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_DevolucionCliente.Listar: " + ex.Message);
                return new List<DEVOLUCIONES_CLIENTES>();
            }
        }

        public List<DETALLE_DEVOLUCIONES_CLIENTES> ObtenerDetalle(int idDevolucionCliente)
        {
            try
            {
                // Traemos el detalle de una devolución específica.
                // Esta es la consulta de TRAZABILIDAD:
                // DetalleDev -> Lote -> Producto
                return db.DETALLE_DEVOLUCIONES_CLIENTES
                         .Include("LOTES")
                         .Include("LOTES.PRODUCTOS")
                         .Where(dd => dd.IdDevolucionCliente == idDevolucionCliente)
                         .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_DevolucionCliente.ObtenerDetalle: " + ex.Message);
                return new List<DETALLE_DEVOLUCIONES_CLIENTES>();
            }
        }
    }
}