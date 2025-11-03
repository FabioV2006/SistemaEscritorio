using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Cliente
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        public List<CLIENTES> Listar()
        {
            try
            {
                return db.CLIENTES.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<CLIENTES>();
            }
        }

        public int Registrar(CLIENTES obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            try
            {
                db.CLIENTES.Add(obj);
                db.SaveChanges();
                Mensaje = "Cliente registrado con éxito.";
                return obj.IdCliente;
            }
            catch (Exception ex)
            {
                Mensaje = "Error al registrar el cliente: " + ex.Message;
                return 0;
            }
        }

        public bool Editar(CLIENTES obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            try
            {
                var clienteExistente = db.CLIENTES.Find(obj.IdCliente);
                if (clienteExistente == null)
                {
                    Mensaje = "Cliente no encontrado.";
                    return false;
                }

                clienteExistente.Documento = obj.Documento;
                clienteExistente.RazonSocial = obj.RazonSocial;
                clienteExistente.NombreComercial = obj.NombreComercial;
                clienteExistente.Correo = obj.Correo;
                clienteExistente.Telefono = obj.Telefono;
                clienteExistente.DireccionEnvio = obj.DireccionEnvio;
                clienteExistente.Estado = obj.Estado;

                db.SaveChanges();
                Mensaje = "Cliente actualizado con éxito.";
                return true;
            }
            catch (Exception ex)
            {
                Mensaje = "Error al editar el cliente: " + ex.Message;
                return false;
            }
        }

        // Dejamos el de Eliminar/Desactivar para después, 
        // por ahora nos centramos en Registrar y Editar.
    }
}