using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Problema_1._5.Domain
{
        public class Factura
        {
            public int Id { get; set; }
            public string NroFactura { get; set; } = "";
            public DateTime Fecha { get; set; } = DateTime.Now;
            public string Cliente { get; set; } = "";
            public int FormaPagoId { get; set; }
            public bool Activo { get; set; } = true;

            private readonly List<DetalleFactura> _detalles = new();
            public IReadOnlyList<DetalleFactura> Detalles => _detalles.AsReadOnly();

            public Factura() { }

            public Factura(string nro, DateTime fecha, string cliente, int formaPagoId)
            {
                NroFactura = nro;
                Fecha = fecha;
                Cliente = cliente;
                FormaPagoId = formaPagoId;
            }

            // regla: si se agrega el mismo artículo, se acumula la cantidad
            public void AgregarDetalle(int articuloId, int cantidad)
            {
                if (cantidad <= 0) throw new ArgumentException("Cantidad debe ser > 0");
                var existente = _detalles.FirstOrDefault(d => d.ArticuloId == articuloId);
                if (existente == null)
                    _detalles.Add(new DetalleFactura { ArticuloId = articuloId, Cantidad = cantidad });
                else
                    existente.Cantidad += cantidad;
            }

            public void RemoverDetalle(int articuloId) => _detalles.RemoveAll(d => d.ArticuloId == articuloId);

            public override string ToString() => $"{NroFactura} - {Cliente} - {Fecha:d} - Items:{_detalles.Count}";
        }
    
}
