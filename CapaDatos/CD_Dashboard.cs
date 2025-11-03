using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Dashboard
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // KPI 1: Ventas del Mes
        public decimal ObtenerTotalVentasMes()
        {
            try
            {
                int mesActual = DateTime.Now.Month;
                int anioActual = DateTime.Now.Year;

                // Suma el MontoTotal de todas las ventas de este mes y año.
                // Usamos (decimal?) y ?? 0 para evitar errores si no hay ventas (Sum devuelve null).
                decimal? total = db.VENTAS
                                   .Where(v => v.FechaRegistro.Value.Month == mesActual &&
                                               v.FechaRegistro.Value.Year == anioActual)
                                   .Sum(v => (decimal?)v.MontoTotal);

                return total ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Dashboard.ObtenerTotalVentasMes: " + ex.Message);
                return 0;
            }
        }

        // KPI 2: Clientes Activos
        public int ObtenerClientesActivos()
        {
            try
            {
                // Cuenta todos los clientes donde Estado es 'true'
                return db.CLIENTES.Count(c => c.Estado == true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Dashboard.ObtenerClientesActivos: " + ex.Message);
                return 0;
            }
        }

        // KPI 3: Lotes por Vencer
        public int ObtenerLotesPorVencer(int dias)
        {
            try
            {
                DateTime hoy = DateTime.Now.Date;
                DateTime fechaLimite = hoy.AddDays(dias);

                // Cuenta lotes que tienen stock, no están vencidos hoy, 
                // pero sí vencen en los próximos 'dias'.
                return db.LOTES.Count(l => l.Stock > 0 &&
                                           l.FechaVencimiento > hoy &&
                                           l.FechaVencimiento <= fechaLimite);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Dashboard.ObtenerLotesPorVencer: " + ex.Message);
                return 0;
            }
        }

        // KPI 4: Productos con Stock Crítico
        public int ObtenerStockCritico(int limiteStock)
        {
            try
            {
                // Cuenta cuántos PRODUCTOS (distintos) tienen al menos un lote
                // con stock por debajo del límite.
                return db.LOTES
                         .Where(l => l.Stock > 0 && l.Stock < limiteStock)
                         .Select(l => l.IdProducto) // Seleccionamos solo el IdProducto
                         .Distinct() // Contamos cada producto una sola vez
                         .Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Dashboard.ObtenerStockCritico: " + ex.Message);
                return 0;
            }
        }
    }
}