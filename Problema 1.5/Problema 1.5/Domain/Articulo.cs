using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Problema_1._5.Domain
{
    public class Articulo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public decimal PrecioUnitario { get; set; }

        public override string ToString() => $"{Id} - {Nombre} - {PrecioUnitario:C}";

    }
}
