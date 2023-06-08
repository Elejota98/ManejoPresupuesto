using Dapper;
using ManejoPresupuesto.Models;
using System.Data;
using System.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TipoCuentas tipoCuentas);
        Task Borrar(int id);
        Task Crear(TipoCuentas tipoCuentas);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<TipoCuentas> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<TipoCuentas>> ObtenerTiposCuentas(int usuarioId);
        Task Ordenar(IEnumerable<TipoCuentas> tipoCuentas);
    }
    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;
        public RepositorioTiposCuentas( IConfiguration configuration)
        {
            //Creamos la conexion a la base de datos 

            connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        //2. Creamos el primer metodo para insertar a la base de datos 

        public async Task Crear (TipoCuentas tipoCuentas)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>
                                                    ("TiposCuentasInsertar",
                                                    new{  UsuarioId = tipoCuentas.UsuarioId,
                                                       nombre = tipoCuentas.Nombre },
                                                    //El New para crear un objeto anonimo, es decir enviarle solo algunos parametros a la base de datos
                                                       commandType: System.Data.CommandType.StoredProcedure);
                                                   
            tipoCuentas.Id = id;
        }

        //3. Verificar si ya existe el tipo cuenta 

        public async Task<bool> Existe(string nombre,int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            // QueryFirstOrDefaultAsync trae lo primero que encuentre o un valor por defecto en este caso un entero, el valor por defecto de un entero es 0
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM TIPOSCUENTAS
                                                                    WHERE Nombre=@Nombre AND UsuarioId=@UsuarioId", new {nombre,usuarioId});

            return existe == 1;

        }

        public async Task<IEnumerable<TipoCuentas>>ObtenerTiposCuentas(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TipoCuentas>(@"SELECT Id,Nombre,Orden,UsuarioId FROM TiposCuentas
                                                            where UsuarioId=@UsuarioId ORDER BY Orden", new { usuarioId }); //Voy a mapear los resultados que tengo en la base de datos a la clase que le indique en <>
            
        }

        //4. Actualizar tipos cuentas

        public async Task Actualizar(TipoCuentas tipoCuentas)
        {
            using var connection = new SqlConnection(connectionString);
            //ExecuteAsync permite ejecutar un query sin tener que retornar nada
            await connection.ExecuteAsync(@"UPDATE TiposCuentas 
                                            SET Nombre=@Nombre where Id=@Id", tipoCuentas);
        }


        //5. Antes de actualizar las tipos cuentas primero debemos obtener la cuentas por id segun el usuario

        public async Task<TipoCuentas> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuentas>(@"SELECT id,Nombre,Orden FROM TiposCuentas WHERE Id=@Id AND UsuarioId=@UsuarioId", new {id,usuarioId});
        }
       

        //6. Metodo para borrar tipos cuentas

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE TiposCuentas WHERE Id=@Id",new {id});
        }

        //7. Metodo para guardar el orden de los tipos de cuentas

        public async Task Ordenar (IEnumerable<TipoCuentas> tipoCuentas)
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden where Id = @Id;";
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(query, tipoCuentas);
        }

    }
}
