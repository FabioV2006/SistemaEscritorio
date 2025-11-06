using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace CapaDatos
{
    // --- CLASES AUXILIARES (Sin cambios) ---
    public class ReporteVentaMensual
    {
        public string MesAnio { get; set; }
        public decimal TotalVendido { get; set; }
    }
    public class ReporteTopProducto
    {
        public string Producto { get; set; }
        public int Cantidad { get; set; }
    }
    public class ReporteVentaCategoria
    {
        public string Categoria { get; set; }
        public decimal TotalVendido { get; set; }
    }

    // --- CLASE PRINCIPAL DE CD_DASHBOARD ---
    public class CD_Dashboard
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // ======================================================
        // === MÉTODOS KPI EXISTENTES (TU CÓDIGO - Sin cambios) ===
        // ======================================================
        public decimal ObtenerTotalVentasMes()
        {
            try
            {
                int mesActual = DateTime.Now.Month;
                int anioActual = DateTime.Now.Year;

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

        public int ObtenerClientesActivos()
        {
            try
            {
                return db.CLIENTES.Count(c => c.Estado == true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Dashboard.ObtenerClientesActivos: " + ex.Message);
                return 0;
            }
        }

        public int ObtenerLotesPorVencer(int dias)
        {
            try
            {
                DateTime hoy = DateTime.Now.Date;
                DateTime fechaLimite = hoy.AddDays(dias);

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

        public int ObtenerStockCritico(int limiteStock)
        {
            try
            {
                return db.LOTES
                         .Where(l => l.Stock > 0 && l.Stock < limiteStock)
                         .Select(l => l.IdProducto)
                         .Distinct()
                         .Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Dashboard.ObtenerStockCritico: " + ex.Message);
                return 0;
            }
        }

        // ======================================================
        // === NUEVOS MÉTODOS PARA GRÁFICOS (CORREGIDOS) ===
        // ======================================================

        public List<ReporteVentaMensual> ObtenerVentasMensuales(DateTime fechaInicio, DateTime fechaFin)
        {
            // (Este método estaba bien)
            try
            {
                var data = db.VENTAS
                    .Where(v => v.FechaRegistro.Value >= fechaInicio && v.FechaRegistro.Value <= fechaFin)
                    .GroupBy(v => new {
                        Year = v.FechaRegistro.Value.Year,
                        Month = v.FechaRegistro.Value.Month
                    })
                    .Select(g => new {
                        Key = g.Key,
                        TotalVendido = g.Sum(v => v.MontoTotal ?? 0)
                    })
                    .OrderBy(x => x.Key.Year)
                    .ThenBy(x => x.Key.Month)
                    .ToList();

                return data.Select(g => new ReporteVentaMensual
                {
                    MesAnio = $"{g.Key.Year}-{g.Key.Month.ToString("D2")}",
                    TotalVendido = g.TotalVendido
                })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Dashboard.ObtenerVentasMensuales: " + ex.Message);
                return new List<ReporteVentaMensual>();
            }
        }

        public List<ReporteTopProducto> ObtenerTopProductos(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var data = db.DETALLE_VENTAS
                    // CORRECCIÓN: De PRODUCTO a PRODUCTOS (plural)
                    .Include(dv => dv.LOTES.PRODUCTOS)
                    .Where(dv => dv.VENTAS.FechaRegistro.Value >= fechaInicio && dv.VENTAS.FechaRegistro.Value <= fechaFin)
                    // CORRECCIÓN: De PRODUCTO a PRODUCTOS (plural)
                    .GroupBy(dv => dv.LOTES.PRODUCTOS.Nombre)
                    .Select(g => new {
                        Producto = g.Key,
                        TotalUnidades = g.Sum(dv => dv.Cantidad ?? 0)
                    })
                    .OrderByDescending(x => x.TotalUnidades)
                    .Take(5)
                    .ToList();

                return data.Select(x => new ReporteTopProducto
                {
                    Producto = x.Producto,
                    Cantidad = (int)x.TotalUnidades
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Dashboard.ObtenerTopProductos: " + ex.Message);
                return new List<ReporteTopProducto>();
            }
        }

        public List<ReporteVentaCategoria> ObtenerVentasCategoria(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var data = db.DETALLE_VENTAS
                    // CORRECCIÓN: De PRODUCTO.CATEGORIA a PRODUCTOS.CATEGORIA (plural.singular)
                    .Include(dv => dv.LOTES.PRODUCTOS.CATEGORIAS)
                    .Where(dv => dv.VENTAS.FechaRegistro.Value >= fechaInicio && dv.VENTAS.FechaRegistro.Value <= fechaFin)
                    // CORRECCIÓN: De PRODUCTO.CATEGORIA a PRODUCTOS.CATEGORIA (plural.singular)
                    .GroupBy(dv => dv.LOTES.PRODUCTOS.CATEGORIAS.Descripcion)
                    .Select(g => new {
                        Categoria = g.Key,
                        TotalVendido = g.Sum(dv => dv.SubTotal ?? 0)
                    })
                    .OrderByDescending(x => x.TotalVendido)
                    .Take(5)
                    .ToList();

                return data.Select(x => new ReporteVentaCategoria
                {
                    Categoria = x.Categoria,
                    TotalVendido = x.TotalVendido
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Dashboard.ObtenerVentasCategoria: " + ex.Message);
                return new List<ReporteVentaCategoria>();
            }
        }
    }
}