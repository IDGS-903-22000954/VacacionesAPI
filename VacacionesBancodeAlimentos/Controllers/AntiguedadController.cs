using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VacacionesBancodeAlimentos.Context;
using VacacionesBancodeAlimentos.Interfaces;
using VacacionesBancodeAlimentos.Model;

namespace VacacionesBancodeAlimentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AntiguedadController : ControllerBase
    {
        private readonly IEmpleadoAntiguedadService _empleadoService;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ViewsDbContext _viewsContext;

        public AntiguedadController(IEmpleadoAntiguedadService empleadoService, ApplicationDbContext applicationDbContext, ViewsDbContext viewsDbContext)
        {
            _empleadoService = empleadoService;
            _applicationDbContext = applicationDbContext;
            _viewsContext = viewsDbContext;
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
            return Ok("Antiguedades actualizadas");
        }

        [HttpPut]
        [Route("actualizarAntiguedad/{id}")]
        public async Task<IActionResult> Actualizar([FromBody] DateTime fecha, string id)
        {
            // Buscamos en ambas tablas
            var antiguedad = await _applicationDbContext.AntiguedadEmpleados.FindAsync(id);
            var empleadoExistente = await _viewsContext.EmpleadosNominas.FindAsync(id);

            // Si el empleado no existe en la vista/maestra de nómina, no podemos hacer nada
            if (empleadoExistente == null)
            {
                return NotFound("El empleado no existe en el sistema de nómina.");
            }

            try
            {
                if (antiguedad == null)
                {
                    // ESCENARIO: El registro NO existe en la tabla AntiguedadEmpleados. Lo CREAMOS.
                    var nuevaAntiguedad = new AntiguedadEmpleados // Sustituye por el nombre real de tu clase modelo
                    {
                        EmpleadoId = id, // Asegúrate de asignar la llave primaria
                        FechaContrato = fecha
                    };

                    _applicationDbContext.AntiguedadEmpleados.Add(nuevaAntiguedad);
                }
                else
                {
                    // ESCENARIO: El registro SÍ existe. Solo ACTUALIZAMOS.
                    antiguedad.FechaContrato = fecha;
                    _applicationDbContext.AntiguedadEmpleados.Update(antiguedad);
                }

                // Guardamos cambios y ejecutamos la lógica general
                await _applicationDbContext.SaveChangesAsync();
                await _empleadoService.ActualizarAntiguedadGeneralAsync();

                return Ok(new { mensaje = "Fecha procesada con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar el registro.");
            }
        }
    }
}
