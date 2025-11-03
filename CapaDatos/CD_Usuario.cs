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
        public List<USUARIOS> Listar()
        {
            try
            {
                // Incluimos ROLES para mostrar el nombre del rol en la grilla
                return db.USUARIOS.Include("ROLES").ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<USUARIOS>();
            }
        }

        public int Registrar(USUARIOS obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            try
            {
                // Validación simple para evitar duplicados
                if (db.USUARIOS.Any(u => u.Documento == obj.Documento))
                {
                    Mensaje = "Ya existe un usuario con el mismo documento.";
                    return 0;
                }

                db.USUARIOS.Add(obj);
                db.SaveChanges();
                Mensaje = "Usuario registrado con éxito.";
                return obj.IdUsuario;
            }
            catch (Exception ex)
            {
                Mensaje = "Error al registrar el usuario: " + ex.Message;
                return 0;
            }
        }

        public bool Editar(USUARIOS obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            try
            {
                var usuarioExistente = db.USUARIOS.Find(obj.IdUsuario);
                if (usuarioExistente == null)
                {
                    Mensaje = "Usuario no encontrado.";
                    return false;
                }

                // Validación de duplicados (excluyendo al usuario actual)
                if (db.USUARIOS.Any(u => u.Documento == obj.Documento && u.IdUsuario != obj.IdUsuario))
                {
                    Mensaje = "Ya existe un usuario con ese documento.";
                    return false;
                }

                usuarioExistente.Documento = obj.Documento;
                usuarioExistente.NombreCompleto = obj.NombreCompleto;
                usuarioExistente.Correo = obj.Correo;
                usuarioExistente.Clave = obj.Clave; // Considera encriptar esto
                usuarioExistente.IdRol = obj.IdRol;
                usuarioExistente.Estado = obj.Estado;

                db.SaveChanges();
                Mensaje = "Usuario actualizado con éxito.";
                return true;
            }
            catch (Exception ex)
            {
                Mensaje = "Error al editar el usuario: " + ex.Message;
                return false;
            }
        }
    }
}