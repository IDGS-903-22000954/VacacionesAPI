using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using VacacionesBancodeAlimentos.Context;
using VacacionesBancodeAlimentos.Interfaces;
using VacacionesBancodeAlimentos.Model;

namespace VacacionesBancodeAlimentos.Controllers
{
    [Authorize]
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
        [Route("antiguedadGeneral")]
        public async Task<IActionResult> General()
        {
            try
            {
                // Este proceso puede tardar si hay muchos empleados, 
                // se recomienda correrlo de forma asíncrona o con un timeout largo.
                await _empleadoService.ActualizarAntiguedadGeneralAsync();
                return Ok(new { mensaje = "Proceso de actualización masiva finalizado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error en el proceso general: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("actualizarAntiguedad/{id}")]
        public async Task<IActionResult> Actualizar([FromBody] DateTime fecha, string id)
        {
            var empleadoExistente = await _viewsContext.EmpleadosNominas.FindAsync(id);
            if (empleadoExistente == null) return NotFound("El empleado no existe.");

            // Usamos el contexto de EF para la transacción
            using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                var antiguedad = await _applicationDbContext.AntiguedadEmpleados.FindAsync(id);

                if (antiguedad == null)
                {
                    antiguedad = new AntiguedadEmpleados { EmpleadoId = id, FechaContrato = fecha };
                    _applicationDbContext.AntiguedadEmpleados.Add(antiguedad);
                }
                else
                {
                    antiguedad.FechaContrato = fecha;
                    _applicationDbContext.AntiguedadEmpleados.Update(antiguedad);
                }

                await _applicationDbContext.SaveChangesAsync();

                // Llamamos al servicio pasando la transacción de Dapper
                await _empleadoService.ActualizarAntiguedadPorEmpleadoAsync(id, transaction.GetDbTransaction());

                await transaction.CommitAsync();
                return Ok(new { mensaje = "Proceso exitoso" });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error al procesar el registro.");
            }
        }
    }
}
