using Dapper;
using ManejoPresupuesto.Models;
using System.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta obtenerTransaccionesPorCuenta);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioTransacciones: IRepositorioTransacciones
    {
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"Transacciones_Insertar",
                new
                {
                    transaccion.UsuarioId,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CuentaId,
                    transaccion.CategoriaId,
                    transaccion.Nota
                },
                commandType: System.Data.CommandType.StoredProcedure);
                transaccion.Id = id;
        }

        //Actualizar

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior,
            int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Actualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        //Obtener transaccion PorId

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleOrDefaultAsync<Transaccion>($"SELECT Transacciones.*,ca.TipoOperacionId FROM Transacciones" +
                                                                        $"  INNER JOIN Categorias ca on ca.Id = Transacciones.CategoriaId " +
                                                                        $"WHERE Transacciones.id=@Id and Transacciones.UsuarioId=@UsuarioId",
                                                                        new {id,usuarioId});
        }

        //Metodo para borrar

        public async Task Borrar (int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar",
                new
                {
                    id
                }, commandType: System.Data.CommandType.StoredProcedure);
        }
        #region Reportes 
        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta obtenerTransaccionesPorCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"SELECT T.Id, T.FechaTransaccion, C.Nombre AS Categorias, CU.Nombre AS Cuenta, C.TipoOperacionId FROM Transacciones T INNER JOIN Categorias C ON C.Id=T.CategoriaId
                                                            INNER JOIN CUENTAS CU ON CU.Id= T.CuentaId
                                                            WHERE T.CuentaId=@CuentaId AND T.UsuarioId=@UsuarioId AND FechaTransaccion
                                                            BETWEEN @FechaInicio and @FechaFin", obtenerTransaccionesPorCuenta);

        }
        #endregion
    }
}
