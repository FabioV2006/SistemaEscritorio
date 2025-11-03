using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Rol
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        public List<ROLES> Listar()
        {
            try
            {
                // Incluimos los permisos para saber cuántos tiene cada rol
                return db.ROLES.Include("PERMISOS").ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CD_Rol.Listar: " + ex.Message);
                return new List<ROLES>();
            }
        }

        public int Registrar(ROLES obj, List<PERMISOS> permisos, out string Mensaje)
        {
            Mensaje = string.Empty;
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Registrar el Rol para obtener su ID
                    db.ROLES.Add(obj);
                    db.SaveChanges();
                    int idRolGenerado = obj.IdRol;

                    // 2. Asignar el ID del rol a cada permiso
                    foreach (PERMISOS p in permisos)
                    {
                        p.IdRol = idRolGenerado;
                    }
                    db.PERMISOS.AddRange(permisos);
                    db.SaveChanges();

                    // 3. Confirmar la transacción
                    transaction.Commit();
                    Mensaje = "Rol registrado con éxito.";
                    return idRolGenerado;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Mensaje = "Error al registrar el rol: " + ex.Message;
                    return 0;
                }
            }
        }

        public bool Editar(ROLES obj, List<PERMISOS> permisos, out string Mensaje)
        {
            Mensaje = string.Empty;
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var rolExistente = db.ROLES.Find(obj.IdRol);
                    if (rolExistente == null)
                    {
                        Mensaje = "Rol no encontrado.";
                        return false;
                    }

                    // 1. Actualizar la descripción del Rol
                    rolExistente.Descripcion = obj.Descripcion;
                    db.SaveChanges();

                    // 2. Borrar todos los permisos ANTIGUOS de ese rol
                    var permisosAntiguos = db.PERMISOS.Where(p => p.IdRol == obj.IdRol);
                    db.PERMISOS.RemoveRange(permisosAntiguos);
                    db.SaveChanges();

                    // 3. Agregar todos los permisos NUEVOS
                    foreach (PERMISOS p in permisos)
                    {
                        p.IdRol = obj.IdRol; // Asegurarse de que el ID es correcto
                    }
                    db.PERMISOS.AddRange(permisos);
                    db.SaveChanges();

                    // 4. Confirmar la transacción
                    transaction.Commit();
                    Mensaje = "Rol y permisos actualizados con éxito.";
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Mensaje = "Error al editar el rol: " + ex.Message;
                    return false;
                }
            }
        }
    }
}