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
        private readonly ApplicationDbContext _appContext;
        private readonly ViewsDbContext _viewsContext;

        public SolicitudController (ApplicationDbContext appContext, ViewsDbContext viewsContext)
        {
            _appContext = appContext;
            _viewsContext = viewsContext;
        }

        [HttpGet]
        [Route("GetSolicitudes")]
        public async Task<IActionResult> Get()
        {
            var Solicitudes = await _appContext.Solicitudes.ToListAsync();
            return Ok(Solicitudes);
        }

        [HttpGet]
        [Route("GetPendientes")]
        public async Task<IActionResult> Pendientes()
        {
            var solicitudesLocales = await _appContext.Solicitudes
                .Include(s => s.SolicitudFechas)
                .Where(s => s.Estatus == 'p')
                .ToListAsync();

            var idsEmpleados = solicitudesLocales.Select(s => s.IdEmpleado).Distinct().ToList();
            var empleadosNominas = await _viewsContext.EmpleadosNominas
                .Where(e => idsEmpleados.Contains(e.CodigoEmpleado))
                .ToDictionaryAsync(e => e.CodigoEmpleado, e => e);

            foreach (var solicitud in solicitudesLocales)
            {
                if (empleadosNominas.TryGetValue(solicitud.IdEmpleado, out var infoEmpleado))
                {
                    solicitud.Empleado = infoEmpleado;
                }
            }

            return Ok(solicitudesLocales);
        }

        [HttpPost]
        [Route("PostSolicitud")]
        public async Task<IActionResult> NewSolicitud([FromBody] Solicitud solicitud)
        {
            await _appContext.Solicitudes.AddAsync(solicitud);
            await _appContext.SaveChangesAsync();

            return Ok(solicitud);
        }

        [HttpDelete]
        [Route("CancelarSolicitud/{id:int}")]
        public async Task<IActionResult> UpdateSolicitud(int id)
        {
            var solicitud = await _appContext.Solicitudes
                .Include(s => s.SolicitudFechas)
                .FirstOrDefaultAsync(s => s.IdSolicitud == id);

            if (solicitud == null) return NotFound("Solicitud no encontrada.");

            if (solicitud.Estatus == 'c') return BadRequest("La solicitud ya está cancelada.");

            try
            {
                if (solicitud.Estatus == 'a')
                {
                    int diasARestaurar = solicitud.SolicitudFechas.Count;

                    var registroVacaciones = await _appContext.Vacaciones
                        .Where(v => v.IdEmpleado == solicitud.IdEmpleado)
                        .OrderByDescending(v => v.Periodo)
                        .FirstOrDefaultAsync();

                    if (registroVacaciones != null)
                    {
                        registroVacaciones.DiasDevueltos += diasARestaurar;

                        _appContext.Vacaciones.Update(registroVacaciones);
                    }
                }

                solicitud.Estatus = 'c';
                _appContext.Solicitudes.Update(solicitud);

                await _appContext.SaveChangesAsync();

                return Ok("Solicitud cancelada con éxito.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("AceptarSolicitud/{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateFormato([FromRoute] int id, IFormFile formato)
        {
            var solicitud = await _appContext.Solicitudes
                .Include(s => s.SolicitudFechas)
                .FirstOrDefaultAsync(s => s.IdSolicitud == id);

            if (solicitud == null) return NotFound();
            if (formato == null || formato.Length == 0) return BadRequest("Archivo vacío.");

            try
            {
                string rootPath = AppContext.BaseDirectory;
                string carpetaDestino = Path.Combine(rootPath, "DocumentosSolicitudes");
                if (!Directory.Exists(carpetaDestino)) Directory.CreateDirectory(carpetaDestino);

                string idEmpleadoStr = solicitud.IdEmpleado.ToString().PadLeft(3, '0');
                string fechaHoy = DateTime.Now.ToString("ddMMyyyy");
                string nombreArchivo = $"{idEmpleadoStr}_{fechaHoy}{Path.GetExtension(formato.FileName)}";
                string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create, FileAccess.Write))
                {
                    await formato.CopyToAsync(stream);
                }

                int diasPorDescontar = solicitud.SolicitudFechas.Count;
                DateTime fechaActual = DateTime.Now;

                var periodosDisponibles = await _appContext.Vacaciones
                    .Where(v => v.IdEmpleado == solicitud.IdEmpleado && v.DiasRestantes > 0)
                    .OrderBy(v => v.Periodo)
                    .ToListAsync();

                foreach (var periodo in periodosDisponibles)
                {
                    if (diasPorDescontar <= 0) break;
                    DateTime inicioPeriodo = new DateTime(periodo.Periodo, 1, 1);
                    DateTime finVigencia = inicioPeriodo.AddMonths(18);
                    
                    if (fechaActual <= finVigencia)
                    {
                        if (periodo.DiasRestantes >= diasPorDescontar)
                        {
                            periodo.DiasRestantes -= diasPorDescontar;
                            diasPorDescontar = 0;
                        }
                        else
                        {
                            diasPorDescontar -= periodo.DiasRestantes;
                            periodo.DiasRestantes = 0;
                        }
                    }
                }

                solicitud.Formato = nombreArchivo;
                solicitud.Estatus = 'a';

                await _appContext.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Solicitud aprobada con éxito",
                    diasPendientesPorAsignar = diasPorDescontar,
                    archivo = nombreArchivo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("DescargarSolicitud/{formato}")]
        public async Task<IActionResult> DescargarSolicitud(string formato)
        {
            try
            {
                string rootPath = AppContext.BaseDirectory;
                string carpetaDestino = Path.Combine(rootPath, "DocumentosSolicitudes");
                string rutaCompleta = Path.Combine(carpetaDestino, formato);

                if (!System.IO.File.Exists(rutaCompleta))
                {
                    return NotFound("El archivo PDF no se encuentra en el servidor.");
                }

                var bytes = await System.IO.File.ReadAllBytesAsync(rutaCompleta);

                return File(bytes, "application/pdf", formato);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al intentar descargar el archivo: {ex.Message}");
            }
        }
    }
}
