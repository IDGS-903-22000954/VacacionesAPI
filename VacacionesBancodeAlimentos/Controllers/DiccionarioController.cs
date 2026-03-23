using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacacionesBancodeAlimentos.Context;
using VacacionesBancodeAlimentos.Model;

namespace VacacionesBancodeAlimentos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DiccionarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiccionarioController (ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetDiccionario")]
        public async Task<IActionResult> Get()
        {
            var diccionarios = await _context.DiccionarioAsuetos
                .Include(d => d.AsuetosFechas)
                .Select(d => new {
                    d.IdFecha,
                    d.Nombre,
                    Fechas = d.AsuetosFechas.Select(f => new {
                        f.Id,
                        f.Fecha
                    })
                })
                .ToListAsync();

            return Ok(diccionarios);
        }


        [HttpPost]
        [Route("PostDiccionario")]
        public async Task<IActionResult> Post([FromBody] DiccionarioAsuetos diccionario)
        {
            await _context.DiccionarioAsuetos.AddAsync(diccionario);
            await _context.SaveChangesAsync();

            return Ok("Fecha agregada con éxito");
        }

        [HttpPut]
        [Route("PutDiccionario/{id:int}")]
        public async Task<IActionResult> Put([FromBody] string nombre, int id)
        {
            var asuetoExistente = await _context.DiccionarioAsuetos.FindAsync(id);
            if (asuetoExistente == null)
            {
                return NotFound("No se encontró el asueto.");
            }

            asuetoExistente.Nombre = nombre;

            try
            {
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al actualizar el registro.");
            }

            return Ok("Asueto Actualizado con éxito");
        }

        [HttpDelete]
        [Route("DeleteDiccionario/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var FechaEliminar = await _context.DiccionarioAsuetos.FindAsync(id);
            if (FechaEliminar == null)
            {
                return BadRequest("No existe esa fecha");
            }

            _context.DiccionarioAsuetos.Remove(FechaEliminar);
            await _context.SaveChangesAsync();

            return Ok("Fecha eliminada con éxito");
        }
    }
}
