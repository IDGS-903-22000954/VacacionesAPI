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
    public class PaginaController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public PaginaController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var pagina = await _applicationDbContext.Pagina.ToListAsync();
            return Ok(pagina);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PaginaDto p)
        {
            var pagina = await _applicationDbContext.Pagina.FindAsync(1);
            if (pagina == null)
            {
                return NotFound("");
            }
            pagina.Ruta = p.Ruta;

            try
            {
                await _applicationDbContext.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al actualizar el registro.");
            }

            return Ok("Pagina actualizada con éxito");
        }
    }

}
