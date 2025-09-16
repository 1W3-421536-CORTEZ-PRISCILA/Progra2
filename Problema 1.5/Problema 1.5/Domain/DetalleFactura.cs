using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Problema_1._5.Domain
{
    public class DetalleFactura
    {
        public int Id { get; set; }
        public int FacturaId { get; set; }
        public int ArticuloId { get; set; }
        public int Cantidad { get; set; }

        public override string ToString() => $"Articulo:{ArticuloId} Cant:{Cantidad}";

    }
}
