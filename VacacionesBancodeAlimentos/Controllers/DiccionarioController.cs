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
            var Diccionarios = await _context.DiccionarioAsuetos.ToListAsync();
            return Ok(Diccionarios);
        }

        [HttpPost]
        [Route("PostDiccionario")]
        public async Task<IActionResult> Post([FromBody] DiccionarioAsuetos diccionario)
        {
            await _context.DiccionarioAsuetos.AddAsync(diccionario);
            await _context.SaveChangesAsync();

            return Ok("Fecha agregada con éxito");
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
