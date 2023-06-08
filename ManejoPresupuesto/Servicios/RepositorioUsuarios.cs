namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioUsuarios
    {
        int ObtenerUsuarios();
    }
    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        public int ObtenerUsuarios()
        {
            return 1;
        }
    }
}
