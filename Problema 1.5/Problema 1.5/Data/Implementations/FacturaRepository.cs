using Problema_1._5.Data.Interfaces;
using Problema_1._5.Data.Helper;
using Problema_1._5.Domain;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Problema_1._5.Data.Implementations
{
    public class FacturaRepository : IFacturaRepository
    {
        private readonly DataHelper _helper;
        public FacturaRepository(string connectionString) => _helper = DataHelper.GetInstance();

        // Inserta factura y sus detalles en una transacción
        public int CreateFacturaWithDetalles(Factura factura)
        {
            using var conn = _helper.GetOpenConnection();
            using var tran = conn.BeginTransaction();
            try
            {
                // 1) Insertar factura, obtener id
                using (var cmd = new SqlCommand("sp_InsertarFactura", conn, tran))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NroFactura", factura.NroFactura ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fecha", factura.Fecha);
                    cmd.Parameters.AddWithValue("@Cliente", factura.Cliente ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FormaPagoId", factura.FormaPagoId);

                    var outId = new SqlParameter("@IdFactura", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outId);
                    cmd.ExecuteNonQuery();
                    factura.Id = (int)outId.Value;
                }

                // 2) Insertar/actualizar detalles con el SP que suma cantidades
                foreach (var d in factura.Detalles)
                {
                    using var cmdD = new SqlCommand("sp_InsertOrUpdateDetalleFactura", conn, tran);
                    cmdD.CommandType = CommandType.StoredProcedure;
                    cmdD.Parameters.AddWithValue("@FacturaId", factura.Id);
                    cmdD.Parameters.AddWithValue("@ArticuloId", d.ArticuloId);
                    cmdD.Parameters.AddWithValue("@Cantidad", d.Cantidad);
                    cmdD.ExecuteNonQuery();
                }

                tran.Commit();
                return factura.Id;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void AddOrUpdateDetalle(int facturaId, int articuloId, int cantidad)
        {
            using var conn = _helper.GetOpenConnection();
            using var cmd = new SqlCommand("sp_InsertOrUpdateDetalleFactura", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FacturaId", facturaId);
            cmd.Parameters.AddWithValue("@ArticuloId", articuloId);
            cmd.Parameters.AddWithValue("@Cantidad", cantidad);
            cmd.ExecuteNonQuery();
        }

        public Factura? GetFacturaConDetalles(int id)
        {
            using var conn = _helper.GetOpenConnection();
            using var cmd = new SqlCommand("sp_GetFacturaConDetalles", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FacturaId", id);

            using var reader = cmd.ExecuteReader();
            Factura? factura = null;

            if (reader.Read())
            {
                factura = new Factura
                {
                    Id = reader.GetInt32(0),
                    NroFactura = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Fecha = reader.GetDateTime(2),
                    Cliente = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    FormaPagoId = reader.GetInt32(4),
                    Activo = reader.GetBoolean(5)
                };
            }

            if (factura == null) return null;

            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var d = new DetalleFactura
                    {
                        Id = reader.GetInt32(0),
                        FacturaId = reader.GetInt32(1),
                        ArticuloId = reader.GetInt32(2),
                        Cantidad = reader.GetInt32(3)
                        // columnas 4 y 5 son nombre y precio del artículo si quieres usarlas
                    };
                    factura.AgregarDetalle(d.ArticuloId, d.Cantidad);
                }
            }

            return factura;
        }

        public List<Factura> GetAllFacturas()
        {
            var list = new List<Factura>();
            using var conn = _helper.GetOpenConnection();
            using var cmd = new SqlCommand("sp_ListarFacturas", conn) { CommandType = CommandType.StoredProcedure };

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Factura
                {
                    Id = reader.GetInt32(0),
                    NroFactura = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Fecha = reader.GetDateTime(2),
                    Cliente = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    // FormaPago can be read as string if needed
                });
            }
            return list;
        }
    }
}