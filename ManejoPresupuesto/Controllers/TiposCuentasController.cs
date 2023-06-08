using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IRepositorioUsuarios repositorioUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IRepositorioUsuarios repositorioUsuarios)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.repositorioUsuarios = repositorioUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            //3. Obtener los tipos cuentas
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var tiposCuentas = await repositorioTiposCuentas.ObtenerTiposCuentas(usuarioId);
            return View(tiposCuentas);
        }
        public IActionResult Crear()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuentas tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }
            tipoCuenta.UsuarioId = repositorioUsuarios.ObtenerUsuarios();

            //Validación si existe el tipo cuenta
            //var existeTIpocuenta = await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);
            //if (existeTIpocuenta)
            //{
            //    ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe");

            //    return View(tipoCuenta);
            //}
           await repositorioTiposCuentas.Crear(tipoCuenta);
            return RedirectToAction("Index");
        }

        //Validacion por medio de javascript, es una petición get es decir, del navegador hacia el servidor 

        [HttpGet]
        public async Task<IActionResult>VerificarExiste(string nombre)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var existe = await repositorioTiposCuentas.Existe(nombre, usuarioId);
            if (existe)
            {
                return Json($"El nombre {nombre} ya existe");
            }
            return Json(true);
        }

        //3.1 Listar las tipos cuentas por medio del id del usuario

        [HttpGet]
        public async Task<IActionResult>Editar(int id)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var tiposCuentas = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);

            if(tiposCuentas is null)
            {
               return RedirectToAction("NoEncontrado", "Home");
            }
            return View(tiposCuentas);
        }

        //4. Metodo para editar el tipo cuenta

        [HttpPost]
        public async Task<IActionResult>Editar(TipoCuentas tipoCuentas)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var tipoCuentaExiste = repositorioTiposCuentas.ObtenerPorId(tipoCuentas.Id, usuarioId);
            if(tipoCuentaExiste is null)
            {
               return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Actualizar(tipoCuentas);
            return RedirectToAction("Index");
        }

        //5. Creo la accion

        [HttpGet]
        public async Task<IActionResult>Borrar(int id)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var tiposCuentas = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if(tiposCuentas is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(tiposCuentas);
        }

        //6. Crear la acción para borrar por medio de httppost

        [HttpPost]
        public async Task<IActionResult>BorrarTipoCuenta(int id)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var tiposCuentas = repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if(tiposCuentas is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioTiposCuentas.Borrar(id);
            return RedirectToAction("Index");
        }
        //7. Creo un metodo httpPost que va recibir la data cuando se arrastre las filas 
        [HttpPost]
        //Frombody -> recibir los datos del cuerpo del formulario, y los ids son los ids de los registros 
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = repositorioUsuarios.ObtenerUsuarios();
            var tiposCuentas = await repositorioTiposCuentas.ObtenerTiposCuentas(usuarioId);
            var IdsTiposCuentas = tiposCuentas.Select(x => x.Id); //Obtengo todos los ids de los tipos de cuentas del usuario 
            var IdsTiposCuenasNoPerteneceAlUsuario  = ids.Except(IdsTiposCuentas).ToList(); // Acá hago una comparación con los Ids que vienen de
                                                                                            //  la base de datos y los que vienen del Front
                                                                                            //Es decir, verifico los Ids que estén en Ids y que no estén en IdsTiposCuentas
            if (IdsTiposCuenasNoPerteneceAlUsuario.Count > 0)
            {
                return Forbid(); //Prohibido
            }

            var tiposCuentasOrdenados = ids.Select((valor, indice) => new TipoCuentas()
            {
                Id = valor,
                Orden = indice + 1
            }).AsEnumerable();

            await repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);

            return Ok();

        }

    }
}
