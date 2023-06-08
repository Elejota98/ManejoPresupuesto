namespace ManejoPresupuesto.Models
{
    public class IndiceCuentaViewModel
    {
        public string TipoCuenta { get; set; }
        public IEnumerable<Cuenta> Cuentas { get; set; }
        public decimal Balance => Cuentas.Sum(x => x.Balance); //Realizo la suma de la data que trae el IENUMERABLE 

    }
}
