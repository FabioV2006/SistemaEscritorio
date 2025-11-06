using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Venta
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        public int Registrar(VENTAS obj, DataTable dtDetalle, out string Mensaje)
        {
            Mensaje = string.Empty;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Añadir el objeto Venta principal
                    db.VENTAS.Add(obj);
                    db.SaveChanges(); // Guardamos para obtener el IdVenta

                    int idVentaGenerada = obj.IdVenta;

                    // 2. Iterar sobre el DataTable (la grilla de la UI)
                    foreach (DataRow row in dtDetalle.Rows)
                    {
                        int idLote = Convert.ToInt32(row["IdLote"]);
                        int cantidadVendida = Convert.ToInt32(row["Cantidad"]);

                        // 3. Buscar el lote en la BD
                        LOTES lote = db.LOTES.Find(idLote);
                        if (lote == null)
                        {
                            throw new Exception($"El lote con ID {idLote} no fue encontrado.");
                        }

                        // 4. VERIFICACIÓN DE STOCK (CRÍTICO)
                        if (lote.Stock < cantidadVendida)
                        {
                            throw new Exception($"Stock insuficiente para el lote {lote.NumeroLote}. Stock disponible: {lote.Stock}");
                        }

                        // 5. RESTAR EL STOCK
                        lote.Stock -= cantidadVendida;

                        // 6. Crear el Detalle de Venta
                        DETALLE_VENTAS detalleVenta = new DETALLE_VENTAS()
                        {
                            IdVenta = idVentaGenerada,
                            IdLote = idLote,
                            Cantidad = cantidadVendida,
                            PrecioVentaUnitario = Convert.ToDecimal(row["PrecioVentaUnitario"]),
                            SubTotal = Convert.ToDecimal(row["SubTotal"])
                        };
                        db.DETALLE_VENTAS.Add(detalleVenta);
                    }

                    // 7. Guardar todos los cambios (Stock actualizado y Detalles insertados)
                    db.SaveChanges();

                    // 8. Si todo salió bien, confirmar la transacción
                    transaction.Commit();
                    Mensaje = "Venta registrada con éxito.";
                    return idVentaGenerada;
                }
                catch (Exception ex)
                {
                    // 9. Si algo falló (ej. stock insuficiente), revertir todo
                    transaction.Rollback();
                    Mensaje = "Error al registrar la venta: " + ex.Message;
                    return 0;
                }
            }
        }
        public List<VENTAS> Listar()
        {
            try
            {
                // Traemos las ventas e incluimos la data relacionada del Cliente y Usuario
                // que mostraremos en la grilla principal.
                return db.VENTAS
                         .Include("CLIENTES") // Usamos strings para EF6
                         .Include("USUARIOS")
                         .OrderByDescending(v => v.FechaRegistro) // Mostrar las más nuevas primero
                         .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Venta.Listar: " + ex.Message);
                return new List<VENTAS>();
            }
        }

        public List<DETALLE_VENTAS> ObtenerDetalle(int idVenta)
        {
            try
            {
                // Traemos el detalle de una venta específica.
                // Esta es la consulta de TRAZABILIDAD:
                // Detalle -> Lote -> Producto
                return db.DETALLE_VENTAS
                         .Include("LOTES")
                         .Include("LOTES.PRODUCTOS")
                         .Where(dv => dv.IdVenta == idVenta)
                         .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Venta.ObtenerDetalle: " + ex.Message);
                return new List<DETALLE_VENTAS>();
            }
        }
        public VENTAS ObtenerVentaParaDevolucion(string numeroDocumento)
        {
            try
            {
                // 1. Quitamos espacios en blanco al inicio/final del input
                string docBuscado = numeroDocumento?.Trim() ?? "";

                // 2. Validación adicional
                if (string.IsNullOrEmpty(docBuscado))
                {
                    Console.WriteLine("Error: El número de documento está vacío.");
                    return null;
                }

                // 3. Buscar la venta con eager loading completo
                var venta = db.VENTAS
                         .Include("CLIENTES")
                         .Include("USUARIOS")
                         .Include("DETALLE_VENTAS")
                         .Include("DETALLE_VENTAS.LOTES")
                         .Include("DETALLE_VENTAS.LOTES.PRODUCTOS")
                         .FirstOrDefault(v => v.NumeroDocumento.Trim().ToLower() == docBuscado.ToLower());

                // 4. Validación adicional de que se cargaron los detalles
                if (venta != null)
                {
                    // Forzar la carga explícita si no se cargó automáticamente
                    if (venta.DETALLE_VENTAS == null || venta.DETALLE_VENTAS.Count == 0)
                    {
                        Console.WriteLine($"Advertencia: La venta {venta.IdVenta} no tiene detalles cargados.");
                        db.Entry(venta).Collection(v => v.DETALLE_VENTAS).Load();

                        foreach (var detalle in venta.DETALLE_VENTAS)
                        {
                            db.Entry(detalle).Reference(d => d.LOTES).Load();
                            if (detalle.LOTES != null)
                            {
                                db.Entry(detalle.LOTES).Reference(l => l.PRODUCTOS).Load();
                            }
                        }
                    }

                    Console.WriteLine($"Venta encontrada: {venta.NumeroDocumento} con {venta.DETALLE_VENTAS.Count} detalles.");
                }
                else
                {
                    Console.WriteLine($"No se encontró ninguna venta con el número de documento: {docBuscado}");
                }

                return venta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CD_Venta.ObtenerVentaParaDevolucion: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return null;
            }
        }
    }
}