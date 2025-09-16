using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Problema_1._5.Domain;

namespace Problema_1._5.Data.Interfaces
{
    public interface IFacturaRepository
    {
        int CreateFacturaWithDetalles(Factura factura); // inserta factura y detalles en transacción
        void AddOrUpdateDetalle(int facturaId, int articuloId, int cantidad);
        Factura? GetFacturaConDetalles(int id);
        List<Factura> GetAllFacturas();

    }
}
