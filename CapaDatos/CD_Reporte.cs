using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Reporte
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        /// <summary>
        /// Obtiene el detalle de ventas filtrado por un rango de fechas y, opcionalmente, por un cliente.
        /// </summary>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <param name="idCliente">Si es 0, trae todos los clientes.</param>
        /// <returns></returns>
        public List<DETALLE_VENTAS> ReporteVentas(DateTime fechaInicio, DateTime fechaFin, int idCliente)
        {
            try
            {
                // Asegurarnos que la fecha fin incluya todo el día
                DateTime fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);

                // Empezamos consultando desde DETALLE_VENTAS para obtener la máxima granularidad
                var query = db.DETALLE_VENTAS
                              .Include("VENTAS")
                              .Include("VENTAS.CLIENTES")
                              .Include("LOTES")
                              .Include("LOTES.PRODUCTOS")
                              .Where(dv => dv.VENTAS.FechaRegistro >= fechaInicio &&
                                           dv.VENTAS.FechaRegistro <= fechaFinAjustada);

                // Si se especificó un cliente (idCliente no es 0), aplicamos el filtro
                if (idCliente != 0)
                {
                    query = query.Where(dv => dv.VENTAS.IdCliente == idCliente);
                }

                return query.OrderBy(dv => dv.VENTAS.FechaRegistro).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Reporte.ReporteVentas: " + ex.Message);
                return new List<DETALLE_VENTAS>();
            }
        }

        // (Aquí irían los métodos para Reporte Lotes por Vencer y Trazabilidad)
    }
}