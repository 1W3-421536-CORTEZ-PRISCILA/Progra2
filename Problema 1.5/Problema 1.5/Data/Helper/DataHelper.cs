using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data.SqlClient;
using Problema_1._5.Domain;
using System.Data;

namespace Problema_1._5.Data.Helper
{
        
    public class DataHelper
    {
        private static DataHelper _instance;
        private SqlConnection _connection;

        private DataHelper()
        {
            // La cadena de conexión debería estar en Resources o en appsettings.json
            _connection = new SqlConnection(Properties.Resources.CadenaConexionLocal);
        }

        public SqlConnection GetOpenConnection()
        {
            // Usamos la misma cadena de conexión que ya tenés en Resources
            var conn = new SqlConnection(Properties.Resources.CadenaConexionLocal);
            conn.Open();
            return conn;
        }
        public static DataHelper GetInstance()
        {
            if (_instance == null)
                _instance = new DataHelper();

            return _instance;
        }

        // Método para consultas con SP que retornan DataTable
        public DataTable ExecuteSpQuery(string sp, List<SpParameter>? param = null)
        {
            DataTable dt = new DataTable();

            try
            {
                _connection.Open();
                var cmd = new SqlCommand(sp, _connection);
                cmd.CommandType = CommandType.StoredProcedure;

                if (param != null)
                {
                    foreach (var p in param)
                        cmd.Parameters.AddWithValue(p.Name, p.Valor);
                }

                dt.Load(cmd.ExecuteReader());
            }
            catch
            {
                dt = null;
            }
            finally
            {
                _connection.Close();
            }

            return dt;
        }

        // Método para ejecutar SP de inserción/actualización/borrado
        public bool ExecuteSpDml(string sp, List<SpParameter>? param = null)
        {
            bool result;

            try
            {
                _connection.Open();
                var cmd = new SqlCommand(sp, _connection);
                cmd.CommandType = CommandType.StoredProcedure;

                if (param != null)
                {
                    foreach (var p in param)
                        cmd.Parameters.AddWithValue(p.Name, p.Valor);
                }

                result = cmd.ExecuteNonQuery() > 0;
            }
            catch
            {
                result = false;
            }
            finally
            {
                _connection.Close();
            }

            return result;
        }

        // Manejo de transacciones para MAESTRO (Factura) + DETALLE (DetalleFactura)
        public bool ExecuteTransaction(Factura factura)
        {
            _connection.Open();
            SqlTransaction transaction = _connection.BeginTransaction();

            try
            {
                // 1. Insertar la Factura (maestro)
                var cmdFactura = new SqlCommand("SP_INSERTAR_FACTURA", _connection, transaction);
                cmdFactura.CommandType = CommandType.StoredProcedure;

                cmdFactura.Parameters.AddWithValue("@NroFactura", factura.NroFactura);
                cmdFactura.Parameters.AddWithValue("@Fecha", factura.Fecha);
                cmdFactura.Parameters.AddWithValue("@Cliente", factura.Cliente);
                cmdFactura.Parameters.AddWithValue("@FormaPagoId", factura.FormaPagoId);

                // Parámetro de salida para obtener el ID generado
                var outputId = new SqlParameter("@IdFactura", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmdFactura.Parameters.Add(outputId);

                int filasFactura = cmdFactura.ExecuteNonQuery();
                if (filasFactura <= 0)
                {
                    transaction.Rollback();
                    return false;
                }

                int facturaId = (int)outputId.Value;

                // 2. Insertar los detalles
                foreach (var det in factura.Detalles)
                {
                    var cmdDetalle = new SqlCommand("SP_INSERTAR_DETALLE", _connection, transaction);
                    cmdDetalle.CommandType = CommandType.StoredProcedure;

                    cmdDetalle.Parameters.AddWithValue("@FacturaId", facturaId);
                    cmdDetalle.Parameters.AddWithValue("@ArticuloId", det.ArticuloId);
                    cmdDetalle.Parameters.AddWithValue("@Cantidad", det.Cantidad);

                    int filasDetalle = cmdDetalle.ExecuteNonQuery();
                    if (filasDetalle <= 0)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                // 3. Si todo salió bien → COMMIT
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
            finally
            {
                _connection.Close();
            }
        }
    }
}
