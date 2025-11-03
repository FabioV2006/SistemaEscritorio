using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Laboratorio
    {
        private CD_Laboratorio cd_laboratorio = new CD_Laboratorio();

        public List<LABORATORIOS> Listar()
        {
            return cd_laboratorio.Listar();
        }
    }
}