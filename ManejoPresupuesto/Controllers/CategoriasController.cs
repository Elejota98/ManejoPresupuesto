using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IRepositorioUsuarios repositorioUsuarios;

        public CategoriasController(IRepositorioCategorias repositorioCategorias, IRepositorioUsuarios repositorioUsuarios)
        {
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioUsuarios = repositorioUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var categorias = await repositorioCategorias.Obtener(usuarioId);
            return View(categorias);
        }

        public async Task<IActionResult> Crear()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if(!ModelState.IsValid)
            {
                return View(categoria);
            }
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            categoria.UsuarioId= usuarioId;
            await repositorioCategorias.Crear(categoria);
            return RedirectToAction("Index");


        }

        //EDITAR

        public async Task<IActionResult>Editar(int id)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var categoria = await repositorioCategorias.ObtenerPorId(id, usuarioId);
            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(categoria);
        }

        [HttpPost]


    }
}
