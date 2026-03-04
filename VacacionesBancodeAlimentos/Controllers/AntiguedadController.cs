using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VacacionesBancodeAlimentos.Interfaces;

namespace VacacionesBancodeAlimentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AntiguedadController : ControllerBase
    {
        private readonly IEmpleadoAntiguedadService _empleadoService;

        public AntiguedadController(IEmpleadoAntiguedadService empleadoService)
        {
            _empleadoService = empleadoService;
        }

        [HttpPost]
        [Route("fechasRandom")]
        public async Task<IActionResult> Random()
        {
            await _empleadoService.GenerarFechasPruebaAsync();
            return Ok("Fechas generadas");
        }

        [HttpPost]
        [Route("antiguedadGeneral")]
        public async Task<IActionResult> General()
        {
            await _empleadoService.ActualizarAntiguedadGeneralAsync();
            return Ok("Antiguedades alidadas");
        }
    }
}
