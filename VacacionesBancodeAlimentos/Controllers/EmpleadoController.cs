using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            int añoMinimoUtil = DateTime.Now.AddMonths(-18).Year;

            // 1. Carga de datos masiva con filtros
            var todosLosNominas = await _viewsContext.EmpleadosNominas.ToListAsync();

            var todasLasVacaciones = await _appContext.Vacaciones
                .AsNoTracking()
                .Where(v => v.DiasRestantes > 0 && v.Periodo >= añoMinimoUtil)
                .ToListAsync();

            var todasLasAntiguedades = await _appContext.AntiguedadEmpleados.ToListAsync();

            // Traemos las solicitudes con sus fechas para calcular el último día disfrutado
            // Filtramos por estatus 'a' (Aceptadas) para asegurar que sean vacaciones reales
            var todasLasSolicitudesConFechas = await _appContext.Solicitudes
                .Include(s => s.SolicitudFechas)
                .AsNoTracking()
                .ToListAsync();

            // 2. Lookup de Antigüedad
            var antiguedadLookup = todasLasAntiguedades.ToDictionary(a => a.EmpleadoId, a => a);

            // 3. Lookup de Vacaciones
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

            // 4. NUEVO: Lookup de Último Día de Vacaciones Disfrutado
            // Buscamos en las solicitudes aceptadas la fecha más alta dentro de SolicitudFechas
            var ultimoDiaDisfrutadoLookup = todasLasSolicitudesConFechas
                .Where(s => s.Estatus == 'a') // Solo las aceptadas
                .SelectMany(s => s.SolicitudFechas.Select(f => new { s.IdEmpleado, f.Fecha }))
                .GroupBy(x => x.IdEmpleado)
                .ToDictionary(
                    g => g.Key,
                    g => (DateTime?)g.Max(x => x.Fecha)
                );

            // 5. Lookup de Fechas de Petición (Última vez que solicitó, independientemente del estatus)
            var fechasSolicitudLookup = todasLasSolicitudesConFechas
                .GroupBy(s => s.IdEmpleado)
                .ToDictionary(
                    g => g.Key,
                    g => (DateTime?)g.Max(s => s.FechaPeticion)
                );

            // 6. Construcción de respuesta
            var respuesta = todosLosNominas.Select(emp =>
            {
                var tieneAntiguedad = antiguedadLookup.TryGetValue(emp.CodigoEmpleado, out var infoAnt);

                // Intentamos obtener el último día disfrutado del lookup
                ultimoDiaDisfrutadoLookup.TryGetValue(emp.CodigoEmpleado, out var ultimaFechaReal);

                return new EmpleadoDetalleDto
                {
                    CodigoEmpleado = emp.CodigoEmpleado,
                    NombreCompleto = $"{emp.Nombre} {emp.ApellidoPaterno} {emp.ApellidoMaterno}",
                    Puesto = emp.Puesto,
                    Departamento = emp.Dpto,
                    CodigoLider = emp.CodigoLider,
                    FechaContrato = tieneAntiguedad ? infoAnt.FechaContrato : DateTime.MinValue,
                    Antiguedad = tieneAntiguedad ? infoAnt.Antiguedad : 0,

                    PeriodosVacaciones = vacacionesLookup.ContainsKey(emp.CodigoEmpleado)
                                        ? vacacionesLookup[emp.CodigoEmpleado]
                                        : new List<VacacionesDto>(),

                    FechaPeticion = fechasSolicitudLookup.ContainsKey(emp.CodigoEmpleado)
                                    ? fechasSolicitudLookup[emp.CodigoEmpleado]
                                    : null,

                    // Este es el nuevo campo solicitado
                    UltimasVacaciones = ultimaFechaReal
                };
            }).ToList();

            return Ok(respuesta);
        }

        [HttpGet]
        [Route("GetEmpleado/{id}")]
        public async Task<IActionResult> GetEmpleadoDetalle(string id)
        {
            var emp = await _viewsContext.EmpleadosNominas
                .FirstOrDefaultAsync(e => e.CodigoEmpleado == id);

            if (emp == null) return NotFound("Empleado no encontrado.");

            var vacacionesHistoricas = await _appContext.Vacaciones
                .AsNoTracking()
                .Where(v => v.IdEmpleado == id)
                .OrderByDescending(v => v.Periodo)
                .ToListAsync();

            var solicitudesHistoricas = await _appContext.Solicitudes
                .Include(s => s.SolicitudFechas)
                .AsNoTracking()
                .Where(s => s.IdEmpleado == id && !string.IsNullOrEmpty(s.Formato)) // Filtro de formato
                .OrderByDescending(s => s.FechaPeticion)
                .ToListAsync();

            var antiguedad = await _appContext.AntiguedadEmpleados
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.EmpleadoId == id);

            var respuesta = new
            {
                NombreCompleto = $"{emp.Nombre} {emp.ApellidoPaterno} {emp.ApellidoMaterno}",
                FechaContrato = antiguedad?.FechaContrato,
                AntiguedadAnios = antiguedad?.Antiguedad,

                ResumenVacaciones = vacacionesHistoricas.Select(v => new {
                    v.Periodo,
                    v.DiasTotales,
                    v.DiasRestantes
                }),

                // Agregamos el campo Periodo aquí para que el frontend pueda agrupar
                HistoricoSolicitudes = solicitudesHistoricas.Select(s => new {
                    s.IdSolicitud,
                    s.Periodo, // Campo clave para el filtrado en React
                    s.Estatus,
                    s.Formato,
                    TotalDias = s.SolicitudFechas.Count,
                    DetalleFechas = s.SolicitudFechas.Select(f => f.Fecha).OrderBy(f => f)
                })
            };

            return Ok(respuesta);
        }
    }
}
