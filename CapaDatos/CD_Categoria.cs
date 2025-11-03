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
                // Ahora listamos todas para poder ver activas e inactivas en la grilla
                return db.CATEGORIAS.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Categoria.Listar: " + ex.Message);
                return new List<CATEGORIAS>();
            }
        }

        public int Registrar(CATEGORIAS obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            try
            {
                db.CATEGORIAS.Add(obj);
                db.SaveChanges();
                Mensaje = "Categoría registrada con éxito.";
                return obj.IdCategoria;
            }
            catch (Exception ex)
            {
                Mensaje = "Error al registrar: " + ex.Message;
                return 0;
            }
        }

        public bool Editar(CATEGORIAS obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            try
            {
                var item = db.CATEGORIAS.Find(obj.IdCategoria);
                if (item == null)
                {
                    Mensaje = "Categoría no encontrada.";
                    return false;
                }
                item.Descripcion = obj.Descripcion;
                item.Estado = obj.Estado;
                db.SaveChanges();
                Mensaje = "Categoría actualizada con éxito.";
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