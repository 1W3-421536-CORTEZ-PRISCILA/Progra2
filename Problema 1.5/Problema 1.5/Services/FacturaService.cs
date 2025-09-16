using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Problema_1._5.Data.Interfaces;
using Problema_1._5.Domain;

namespace Problema_1._5.Services
{
    public class FacturaService
    {
        
            private readonly IFacturaRepository _repo;
            public FacturaService(IFacturaRepository repo) => _repo = repo;

            // Crear factura (usa el repo que hace la transacción)
            public int CrearFactura(Factura factura)
            {
                // Validaciones básicas
                if (factura == null) throw new ArgumentNullException(nameof(factura));
                if (!factura.Detalles.Any()) throw new ArgumentException("La factura debe tener al menos un detalle.");
                return _repo.CreateFacturaWithDetalles(factura);
            }

            // Agregar un artículo a una factura existente (suma si ya existe)
            public void AgregarItem(int facturaId, int articuloId, int cantidad)
            {
                _repo.AddOrUpdateDetalle(facturaId, articuloId, cantidad);
            }

            public Factura? ObtenerFactura(int id) => _repo.GetFacturaConDetalles(id);

            public List<Factura> ListarFacturas() => _repo.GetAllFacturas();
        
    }
}
