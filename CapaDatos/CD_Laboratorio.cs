using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Laboratorio
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        public List<LABORATORIOS> Listar()
        {
            try
            {
                // Solo listamos laboratorios activos
                return db.LABORATORIOS.Where(l => l.Estado == true).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<LABORATORIOS>();
            }
        }
    }
}