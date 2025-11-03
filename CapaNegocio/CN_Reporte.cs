using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Reporte
    {
        private CD_Reporte cd_reporte = new CD_Reporte();

        public List<DETALLE_VENTAS> ReporteVentas(DateTime fechaInicio, DateTime fechaFin, int idCliente)
        {
            // Regla de negocio: si las fechas están invertidas, las corregimos
            if (fechaFin < fechaInicio)
            {
                var temp = fechaInicio;
                fechaInicio = fechaFin;
                fechaFin = temp;
            }

            // Regla de negocio: no permitir un rango de más de 1 año (ejemplo)
            if ((fechaFin - fechaInicio).TotalDays > 366)
            {
                // Podríamos lanzar una excepción o simplemente acortar el rango
                fechaFin = fechaInicio.AddYears(1);
            }

            return cd_reporte.ReporteVentas(fechaInicio, fechaFin, idCliente);
        }
    }
}