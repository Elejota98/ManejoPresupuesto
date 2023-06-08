using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="El campo es requerido")]
        [StringLength(maximumLength:50, ErrorMessage ="El campo no puede ser mayor a {1} caracteres")]
        public string Nombre { get; set; }
        [Display(Name ="Tipo operación")]
        public TipoOperacion TipoOperacionId { get; set; }
        public int UsuarioId { get; set; }
    }
}
