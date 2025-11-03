using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Producto
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        public List<PRODUCTOS> Listar()
        {
            try
            {
                // Usamos la sintaxis de string de EF6 para .Include()
                return db.PRODUCTOS
                         .Include("CATEGORIAS")
                         .Include("LABORATORIOS")
                         .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Producto.Listar: " + ex.Message);
                return new List<PRODUCTOS>();
            }
        }

        public int Registrar(PRODUCTOS obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            try
            {
                db.PRODUCTOS.Add(obj);
                db.SaveChanges();
                Mensaje = "Producto registrado con éxito.";
                return obj.IdProducto;
            }
            catch (Exception ex)
            {
                Mensaje = "Error al registrar el producto: " + ex.Message;
                return 0;
            }
        }

        public bool Editar(PRODUCTOS obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            try
            {
                var productoExistente = db.PRODUCTOS.Find(obj.IdProducto);
                if (productoExistente == null)
                {
                    Mensaje = "Producto no encontrado.";
                    return false;
                }

                productoExistente.Codigo = obj.Codigo;
                productoExistente.Nombre = obj.Nombre;
                productoExistente.Descripcion = obj.Descripcion;
                productoExistente.IdCategoria = obj.IdCategoria;
                productoExistente.IdLaboratorio = obj.IdLaboratorio;
                productoExistente.RequiereReceta = obj.RequiereReceta;
                productoExistente.Estado = obj.Estado;

                db.SaveChanges();
                Mensaje = "Producto actualizado con éxito.";
                return true;
            }
            catch (Exception ex)
            {
                Mensaje = "Error al editar el producto: " + ex.Message;
                return false;
            }
        }
    }
}