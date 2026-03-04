using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VacacionesBancodeAlimentos.Context;

namespace VacacionesBancodeAlimentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpleadoController : Controller
    {
        private readonly ViewsDbContext _viewsContext;
        private readonly ApplicationDbContext _appContext;

        public EmpleadoController(ViewsDbContext viewsContext, ApplicationDbContext appContext)
        {
            _viewsContext = viewsContext;
            _appContext = appContext;
        }

        [HttpGet]
        [Route("GetEmpleados")]
        public async Task<IActionResult> GetAllEmpleado()
        {
            // Definimos el límite de utilidad (18 meses atrás)
            int añoActual = DateTime.Now.Year;
            int añoMinimoUtil = DateTime.Now.AddMonths(-18).Year;

            // 1. Carga de datos masiva con filtros
            var todosLosNominas = await _viewsContext.EmpleadosNominas.ToListAsync();

            var todasLasVacaciones = await _appContext.Vacaciones
                .AsNoTracking()
                .Where(v => v.DiasRestantes > 0 && v.Periodo >= añoMinimoUtil) // Solo traemos lo útil
                .ToListAsync();

            var todasLasAntiguedades = await _appContext.AntiguedadEmpleados.ToListAsync();

            var todasLasSolicitudes = await _appContext.Solicitudes
                .OrderByDescending(s => s.FechaPeticion)
                .ToListAsync();

            // 2. Lookup de Antigüedad
            var antiguedadLookup = todasLasAntiguedades
                .ToDictionary(a => a.EmpleadoId, a => a);

            // 3. Lookup de Vacaciones (Mapeo optimizado)
            var vacacionesLookup = todasLasVacaciones
                .GroupBy(v => v.IdEmpleado.ToString().Trim())
                .ToDictionary(g => g.Key, g => g.Select(v => new VacacionesDto
                {
                    Periodo = v.Periodo,
                    DiasTotales = v.DiasTotales,
                    DiasRestantes = v.DiasRestantes
                })
                .OrderByDescending(v => v.Periodo)
                .ToList());

            // 4. Lookup de Fechas de Petición
            var fechasSolicitudLookup = todasLasSolicitudes
                .GroupBy(s => s.IdEmpleado)
                .ToDictionary(
                    g => g.Key,
                    g => (DateTime?)g.Max(s => s.FechaPeticion)
                );

            // 5. Construcción de respuesta
            var respuesta = todosLosNominas.Select(emp =>
            {
                var tieneAntiguedad = antiguedadLookup.TryGetValue(emp.CodigoEmpleado, out var infoAnt);

                return new EmpleadoDetalleDto
                {
                    CodigoEmpleado = emp.CodigoEmpleado,
                    NombreCompleto = $"{emp.Nombre} {emp.ApellidoPaterno} {emp.ApellidoMaterno}",
                    Puesto = emp.Puesto,
                    Departamento = emp.Dpto,
                    FechaContrato = tieneAntiguedad ? infoAnt.FechaContrato : DateTime.MinValue,
                    Antiguedad = tieneAntiguedad ? infoAnt.Antiguedad : 0,

                    // Solo incluirá periodos vigentes con días > 0
                    PeriodosVacaciones = vacacionesLookup.ContainsKey(emp.CodigoEmpleado)
                                        ? vacacionesLookup[emp.CodigoEmpleado]
                                        : new List<VacacionesDto>(),

                    FechaPeticion = fechasSolicitudLookup.ContainsKey(emp.CodigoEmpleado)
                                    ? fechasSolicitudLookup[emp.CodigoEmpleado]
                                    : null
                };
            }).ToList();

            return Ok(respuesta);
        }

        [HttpGet]
        [Route("GetEmpleado/{id}")]
        public async Task<IActionResult> GetEmpleadoDetalle(string id)
        {
            // 1. Buscamos la información base en Nómina
            var emp = await _viewsContext.EmpleadosNominas
                .FirstOrDefaultAsync(e => e.CodigoEmpleado == id);

            if (emp == null) return NotFound("Empleado no encontrado.");

            // 2. Cargamos todo el histórico sin filtros de vigencia
            // Usamos Include para traer las fechas de cada solicitud
            var vacacionesHistoricas = await _appContext.Vacaciones
                .AsNoTracking()
                .Where(v => v.IdEmpleado == id)
                .OrderByDescending(v => v.Periodo)
                .ToListAsync();

            var solicitudesHistoricas = await _appContext.Solicitudes
                .Include(s => s.SolicitudFechas) // Trae el desglose de días de cada petición
                .AsNoTracking()
                .Where(s => s.IdEmpleado == id)
                .OrderByDescending(s => s.FechaPeticion)
                .ToListAsync();

            var antiguedad = await _appContext.AntiguedadEmpleados
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.EmpleadoId == id);

            // 3. Mapeo al DTO de respuesta
            var respuesta = new
            {
                CodigoEmpleado = emp.CodigoEmpleado,
                NombreCompleto = $"{emp.Nombre} {emp.ApellidoPaterno} {emp.ApellidoMaterno}",
                Puesto = emp.Puesto,
                Departamento = emp.Dpto,
                FechaContrato = antiguedad?.FechaContrato,
                AntiguedadAnios = antiguedad?.Antiguedad,

                // Mapeo de periodos
                ResumenVacaciones = vacacionesHistoricas.Select(v => new {
                    v.Periodo,
                    v.DiasTotales,
                    v.DiasRestantes,
                    Estado = v.DiasRestantes == 0 ? "Agotado" : "Disponible"
                }),

                // Mapeo de solicitudes con su detalle de fechas
                HistoricoSolicitudes = solicitudesHistoricas.Select(s => new {
                    s.IdSolicitud,
                    s.FechaPeticion,
                    s.Estatus, // p=pendiente, a=aprobado, c=cancelado
                    s.Formato, // Nombre del archivo PDF
                    TotalDias = s.SolicitudFechas.Count,
                    DetalleFechas = s.SolicitudFechas.Select(f => f.Fecha).OrderBy(f => f)
                })
            };

            return Ok(respuesta);
        }
    }
}
