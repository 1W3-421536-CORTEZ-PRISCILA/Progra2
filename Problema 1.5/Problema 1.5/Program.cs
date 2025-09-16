using Problema_1._5.Data.Implementations;
using Problema_1._5.Domain;
using Problema_1._5.Services;


var connectionString = "Server=localhost;Database=Practica01DB;Trusted_Connection=True;TrustServerCertificate=True";

// Instancias
var facturaRepo = new FacturaRepository(connectionString);
var facturaService = new FacturaService(facturaRepo);

// Crear factura de prueba
var factura = new Factura("F-0001", DateTime.Now, "Cliente Demo", formaPagoId: 1);
factura.AgregarDetalle(articuloId: 1, cantidad: 2);
factura.AgregarDetalle(articuloId: 1, cantidad: 3); // se acumula a 5
factura.AgregarDetalle(articuloId: 2, cantidad: 1);

var newId = facturaService.CrearFactura(factura);
Console.WriteLine($"Factura creada Id: {newId}");

// Agregar item a la factura ya creada (prueba AddOrUpdate)
facturaService.AgregarItem(newId, articuloId: 1, cantidad: 2); // sumará 2 más al articulo 1

// Obtener factura con detalles
var f = facturaService.ObtenerFactura(newId);
Console.WriteLine(f);
foreach (var d in f?.Detalles ?? Enumerable.Empty<Problema_1._5.Domain.DetalleFactura>())
    Console.WriteLine(d);
