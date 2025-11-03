using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Categoria
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        public List<CATEGORIAS> Listar()
        {
            try
            {
                // Solo listamos categorías activas
                return db.CATEGORIAS.Where(c => c.Estado == true).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<CATEGORIAS>();
            }
        }
    }
}