using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Usuario
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        public USUARIOS Validar(string documento, string clave)
        {
            try
            {
                USUARIOS usuario = db.USUARIOS
                                     .Include("ROLES")
                                     .FirstOrDefault(u => u.Documento == documento && u.Clave == clave && u.Estado == true);

                return usuario;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}