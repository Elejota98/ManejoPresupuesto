using Dapper;
using ManejoPresupuesto.Models;
using System.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCategorias
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCategorias : IRepositorioCategorias
    {
        private readonly string connectionString;
        public RepositorioCategorias(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            
        }

        //CREAR

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("INSERT INTO Categorias (Nombre,TipoOperacionId,UsuarioId) VALUES(@Nombre,@TipoOperacionId,@UsuarioId);" +
                                                             "SELECT SCOPE_IDENTITY();", categoria);
            categoria.Id = id;
        }

        //LISTAR

        public async Task<IEnumerable<Categoria>>Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(@"SELECT * FROM Categorias WHERE UsuarioId=@UsuarioId", new { usuarioId });
        }
        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);
            //El arroba lo que quiere decir es que yo pueda tener el scrip o la linea dentro de las comillas en varias líneas
            return await connection.QueryAsync<Categoria>(@"SELECT * FROM Categorias WHERE UsuarioId=@UsuarioId and TipoOperacionId=@TipoOperacionId", new { usuarioId, tipoOperacionId }); 
        }

        //EDITAR

        public async Task<Categoria>ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(@"SELECT * FROM Categorias WHERE Id=@Id and UsuarioId=@UsuarioId", new {id,usuarioId});
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Categorias SET Nombre=@Nombre, TipoOperacionId=@TipoOperacionId WHERE Id=@Id", categoria);
        }

        //BORRAR

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE FROM Categorias WHERE Id=@Id", new { id });
        }
    }
}
