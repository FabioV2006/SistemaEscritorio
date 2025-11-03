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
                // Listamos todos
                return db.LABORATORIOS.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Laboratorio.Listar: " + ex.Message);
                return new List<LABORATORIOS>();
            }
        }

        public int Registrar(LABORATORIOS obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            try
            {
                db.LABORATORIOS.Add(obj);
                db.SaveChanges();
                Mensaje = "Laboratorio registrado con éxito.";
                return obj.IdLaboratorio;
            }
            catch (Exception ex)
            {
                Mensaje = "Error al registrar: " + ex.Message;
                return 0;
            }
        }

        public bool Editar(LABORATORIOS obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            try
            {
                var item = db.LABORATORIOS.Find(obj.IdLaboratorio);
                if (item == null)
                {
                    Mensaje = "Laboratorio no encontrado.";
                    return false;
                }
                item.Nombre = obj.Nombre;
                item.Descripcion = obj.Descripcion;
                item.Estado = obj.Estado;
                db.SaveChanges();
                Mensaje = "Laboratorio actualizado con éxito.";
                return true;
            }
            catch (Exception ex)
            {
                Mensaje = "Error al editar: " + ex.Message;
                return false;
            }
        }
    }
}