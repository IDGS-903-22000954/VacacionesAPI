using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacacionesBancodeAlimentos.Context;
using VacacionesBancodeAlimentos.Model;

namespace VacacionesBancodeAlimentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CalculoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetCalculo")]
        public async Task<IActionResult> Get()
        {
            var calculos = await _context.Calculos.ToListAsync();
            return Ok(calculos);
        }

        [HttpPut]
        [Route("PutCalculo")]
        public async Task<IActionResult> Update([FromBody] List<Calculo> Request)
        {
            await _context.Calculos.ExecuteDeleteAsync();
            await _context.Calculos.AddRangeAsync(Request);
            await _context.SaveChangesAsync();

            return Ok(Request);
        }

    }
}
