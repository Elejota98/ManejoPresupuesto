using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IRepositorioUsuarios repositorioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IMapper mapper;

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IRepositorioUsuarios repositorioUsuarios
            , IRepositorioCuentas repositorioCuentas, IMapper mapper)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.repositorioUsuarios = repositorioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var modelo = new CuentaCreacionViewModel();
            modelo.TiposCuentas = await  ObtenerTiposCuentas(usuarioId);
            return View(modelo);
        }

        //Crear la cuenta 

        [HttpPost]
        public async Task<IActionResult>Crear(CuentaCreacionViewModel cuentaCreacionViewModel)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuentaCreacionViewModel.TipoCuentaId,usuarioId);
            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            if (!ModelState.IsValid)
            {
                cuentaCreacionViewModel.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuentaCreacionViewModel);
            }

            await repositorioCuentas.Crear(cuentaCreacionViewModel);
            return RedirectToAction("Index");
        }

        //Creo una funcion aparte para listar las cuentas, para despues solo llamar esta funcion en algun otro metodo 

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioTiposCuentas.ObtenerTiposCuentas(usuarioId);
             return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString())).ToList();
        }

        //3. Creo la accion de index que va a contener el modelo Indexcracionviewmodel 

        public async Task<IActionResult> Index()
        {
            //Priemro me traigo al usuario 
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            //Me traigo las cuentas que le peternece al usuario
            var cuentascontipoCuenta = await repositorioCuentas.BuscarCuentas(usuarioId);
            //Tercero, voy a cargar los datos al modelo que acabe de crear 
            var modelo = cuentascontipoCuenta.GroupBy(x => x.TipoCuenta)//Una vez listado las tipos cuentas hago un select para agruparlos
                .Select(grupo => new IndiceCuentaViewModel
                {
                    TipoCuenta = grupo.Key,
                    Cuentas = grupo.AsEnumerable()
                }).ToList();

            return View(modelo);
        }


        public async Task<IActionResult>Editar(int id)
        {
            var usuarioId =  repositorioUsuarios.ObtenerUsuarios();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var modelo = mapper.Map<CuentaCreacionViewModel>(cuenta);
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            return View(modelo);

        }
        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaEditar)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var cuenta = await repositorioCuentas.ObtenerPorId(cuentaEditar.Id, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuentaEditar.Id, usuarioId);
            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuentas.Actualizar(cuentaEditar);
            return RedirectToAction("Iandex");

        }

        //BORRAR

        public async Task<IActionResult>Borrar(int id)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult>BorrarCuenta(int id)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioCuentas.Borrar(id);
            return RedirectToAction("Index");
        }
    }
}
