using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacacionesBancodeAlimentos.Context;

namespace VacacionesBancodeAlimentos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VacacionesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public VacacionesController (ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetVacaciones")]
        public async Task<IActionResult> GetVacaciones()
        {
            var Vacaciones = await _context.Vacaciones.ToListAsync();
            return Ok(Vacaciones);
        }
    }
}
