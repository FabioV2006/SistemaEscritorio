using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Dashboard
    {
        private CD_Dashboard cd_dashboard = new CD_Dashboard();

        public decimal ObtenerTotalVentasMes()
        {
            return cd_dashboard.ObtenerTotalVentasMes();
        }

        public int ObtenerClientesActivos()
        {
            return cd_dashboard.ObtenerClientesActivos();
        }

        public int ObtenerLotesPorVencer()
        {
            // Regla de Negocio: "Próximo a Vencer" significa en los próximos 90 días.
            return cd_dashboard.ObtenerLotesPorVencer(90);
        }

        public int ObtenerStockCritico()
        {
            // Regla de Negocio: "Stock Crítico" es cuando un producto tiene menos de 10 unidades en un lote.
            return cd_dashboard.ObtenerStockCritico(10);
        }
    }
}