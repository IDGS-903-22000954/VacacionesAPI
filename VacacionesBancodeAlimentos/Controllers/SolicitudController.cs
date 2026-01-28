using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacacionesBancodeAlimentos.Context;
using VacacionesBancodeAlimentos.Model;

namespace VacacionesBancodeAlimentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolicitudController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SolicitudController (ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetSolicitudes")]
        public async Task<IActionResult> Get()
        {
            var Solicitudes = await _context.Solicitudes.ToListAsync();
            return Ok(Solicitudes);
        }

        [HttpPost]
        [Route("NewSolicitud")]
        public async Task<IActionResult> NewSolicitud([FromBody] Solicitud solicitud)
        {
            await _context.Solicitudes.AddAsync(solicitud);
            await _context.SaveChangesAsync();

            return Ok(solicitud);
        }

        [HttpPut]
        [Route("UpdateSolicitud/{id:int}/{estatus}")]
        public async Task<IActionResult> UpdateSolicitud(int id, char estatus)
        {
            var Solicitud = await _context.Solicitudes.FindAsync(id);
            if (Solicitud == null)
            {
                return NotFound();
            }
            Solicitud.Estatus = estatus;
            await _context.SaveChangesAsync();

            return Ok("Se ha actualizado la solicitud con éxito");
        }

        [HttpPut]
        [Route("UpdateFormato/{id:int}/{formato}")]
        public async Task<IActionResult> UpdateFormato(int id, string formato)
        {
            var Solicitud = await _context.Solicitudes.FindAsync(id);
            if (Solicitud == null)
            {
                return NotFound();
            }
            Solicitud.Formato = formato;
            await _context.SaveChangesAsync();
            return Ok("Se ha añadido el formato con éxito");
        }
    }
}
