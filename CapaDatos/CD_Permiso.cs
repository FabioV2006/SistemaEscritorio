using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Permiso
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        public List<PERMISOS> Listar(int IdRol)
        {
            try
            {
                return db.PERMISOS
                    .Where(p => p.IdRol == IdRol)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<PERMISOS>();
            }
        }
    }
}
