using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacacionesBancodeAlimentos.Context;

namespace VacacionesBancodeAlimentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewsController : Controller
    {
        private readonly ViewsDbContext _context;

        public ViewsController(ViewsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("Dpt")]
        public async Task<IActionResult> Dpt()
        {
            var Lista = await _context.DepartamentosNominas.ToListAsync();
            return Ok(Lista);
        }

        [HttpGet]
        [Route("Emp")]
        public async Task<IActionResult> Emp()
        {
            var Lista = await _context.EmpleadosNominas.ToListAsync();
            return Ok(Lista);
        }

        [HttpGet]
        [Route("Pst")]
        public async Task<IActionResult> Pst()
        {
            var Lista = await _context.PuestosNominas.ToListAsync();
            return Ok(Lista);
        }
    }
}
