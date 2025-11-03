using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Proveedor
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        public List<PROVEEDORES> Listar()
        {
            try
            {
                // Incluimos las entidades relacionadas que podamos necesitar (aunque aquí no hay)
                return db.PROVEEDORES.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<PROVEEDORES>();
            }
        }

        public int Registrar(PROVEEDORES obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            try
            {
                db.PROVEEDORES.Add(obj);
                db.SaveChanges();
                Mensaje = "Proveedor registrado con éxito.";
                return obj.IdProveedor;
            }
            catch (Exception ex)
            {
                Mensaje = "Error al registrar el proveedor: " + ex.Message;
                return 0;
            }
        }

        public bool Editar(PROVEEDORES obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            try
            {
                var proveedorExistente = db.PROVEEDORES.Find(obj.IdProveedor);
                if (proveedorExistente == null)
                {
                    Mensaje = "Proveedor no encontrado.";
                    return false;
                }

                proveedorExistente.Documento = obj.Documento;
                proveedorExistente.RazonSocial = obj.RazonSocial;
                proveedorExistente.Correo = obj.Correo;
                proveedorExistente.Telefono = obj.Telefono;
                proveedorExistente.Estado = obj.Estado;

                db.SaveChanges();
                Mensaje = "Proveedor actualizado con éxito.";
                return true;
            }
            catch (Exception ex)
            {
                Mensaje = "Error al editar el proveedor: " + ex.Message;
                return false;
            }
        }
    }
}