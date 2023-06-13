using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Transactions;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionController:Controller
    {
        private readonly IRepositorioUsuarios repositorioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IMapper mapper;

        public TransaccionController(IRepositorioUsuarios repositorioUsuarios, IRepositorioCuentas repositorioCuentas, IRepositorioCategorias repositorioCategorias,
            IRepositorioTransacciones repositorioTransacciones, IMapper mapper)
        {
            this.repositorioUsuarios = repositorioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);

        }

        public async Task<IEnumerable<SelectListItem>>ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuentas.BuscarCuentas(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));

        }

        //Listar categorias

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId =  repositorioUsuarios.ObtenerUsuarios();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }

        private async Task<IEnumerable<SelectListItem>>ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.Obtener(usuarioId, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
        //CREAR

        [HttpPost]
        public async Task<IActionResult>Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            modelo.UsuarioId = usuarioId;
            if(modelo.TipoOperacionId== TipoOperacion.Gasto)
            {
                modelo.Monto *= -1;
            }
            await repositorioTransacciones.Crear(modelo);
            return RedirectToAction("Index");
        }
        //ACTUALIZAR

        [HttpGet]
        public async Task<IActionResult>Editar(int id)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var transaccion = await repositorioTransacciones.ObtenerPorId(id,usuarioId);
            if(transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);
            modelo.MontoAnterior = modelo.Monto;
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.MontoAnterior=modelo.Monto * -1;
            }
            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId,transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categorias = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if (categorias is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var transaccion = mapper.Map<Transaccion>(modelo);
            if(modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1;
            }
            await repositorioTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);
            return RedirectToAction("Index");
        }

    }
}
