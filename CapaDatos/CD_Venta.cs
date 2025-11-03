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
                // ----- LA CORRECCIÓN -----
                // 1. Quitamos espacios en blanco al inicio/final del input
                string docBuscado = numeroDocumento.Trim();

                // 2. Comparamos el texto de la BD (con Trim) y el input (con docBuscado),
                //    ignorando mayúsculas y minúsculas (OrdinalIgnoreCase).
                return db.VENTAS
                         .Include("CLIENTES")
                         .Include("DETALLE_VENTAS")
                         .Include("DETALLE_VENTAS.LOTES")
                         .Include("DETALLE_VENTAS.LOTES.PRODUCTOS")
                         .FirstOrDefault(v => string.Equals(v.NumeroDocumento.Trim(), docBuscado, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Venta.ObtenerVentaParaDevolucion: " + ex.Message);
                return null;
            }
        }
    }
}