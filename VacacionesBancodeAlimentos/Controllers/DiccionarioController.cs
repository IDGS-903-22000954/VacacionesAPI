using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacacionesBancodeAlimentos.Context;
using VacacionesBancodeAlimentos.Model;

namespace VacacionesBancodeAlimentos.Controllers
{
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
            var Diccionarios = await _context.DiccionarioFechas.ToListAsync();
            return Ok(Diccionarios);
        }

        [HttpPost]
        [Route("PostDiccionario")]
        public async Task<IActionResult> Post([FromBody] DiccionarioFechas Fecha)
        {
            await _context.DiccionarioFechas.AddAsync(Fecha);
            await _context.SaveChangesAsync();

            return Ok("Fecha agregada con éxito");
        }

        [HttpDelete]
        [Route("DeleteDiccionario/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var FechaEliminar = await _context.DiccionarioFechas.FindAsync(id);
            if (FechaEliminar == null)
            {
                return BadRequest("No existe esa fecha");
            }

            _context.DiccionarioFechas.Remove(FechaEliminar);
            await _context.SaveChangesAsync();

            return Ok("Fecha eliminada con éxito");
        }
    }
}
