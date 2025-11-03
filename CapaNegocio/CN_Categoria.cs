using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Categoria
    {
        private CD_Categoria cd_categoria = new CD_Categoria();

        public List<CATEGORIAS> Listar()
        {
            return cd_categoria.Listar();
        }
    }
}