using Dapper;
using ManejoPresupuesto.Models;
using System.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(CuentaCreacionViewModel cuentaCreacionViewModel);
        Task Borrar(int id);
        Task<IEnumerable<Cuenta>> BuscarCuentas(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCuentas : IRepositorioCuentas
    {
        private readonly string connectionString;

        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        //1. Metodo para insertar la cuenta

        public async Task Crear (Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"Insert into Cuentas (Nombre,TipoCuentaId,Descripcion,Balance)
                                                        VALUES (@Nombre,@TipoCuentaId,@Descripcion,@Balance);
                                                        SELECT SCOPE_IDENTITY();", cuenta);
            cuenta.Id = id;
        }

        //3. Metodo para traer las cuentas con sus tipos cuentas

        public async Task<IEnumerable<Cuenta>> BuscarCuentas(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Cuenta>(@"SELECT Cuentas.Id, Cuentas.Nombre, Balance,tp.Nombre AS TipoCuenta FROM 
                                                        Cuentas inner join TiposCuentas tp on tp.Id=Cuentas.TipoCuentaId
                                                        where tp.UsuarioId=@UsuarioId
                                                        ORDER BY TP.Orden", new {usuarioId});
        }

        public async Task<Cuenta>ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(@"SELECT Cuentas.Id, Cuentas.Nombre, Balance, Descripcion,tp.Id FROM 
                                                        Cuentas inner join TiposCuentas tp on tp.Id=Cuentas.TipoCuentaId
                                                        where tp.UsuarioId=@UsuarioId AND Cuentas.Id=@Id", new { id, usuarioId });
        }

        //4. Metodo para actualizar las cuentas 

        public async Task Actualizar(CuentaCreacionViewModel cuentaCreacionViewModel)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Cuentas
                                            set Nombre = @Nombre, Balance=@Balance, Descripcion=@Descripcion,
                                            TipoCuentaId=@TipoCuentaId WHERE Id=@Id;", cuentaCreacionViewModel);

        }

        //5. Metodo para borrar la cuenta 

        public async Task Borrar (int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE Cuentas WHERE Id=@Id", new { id });
        }

    }
}
