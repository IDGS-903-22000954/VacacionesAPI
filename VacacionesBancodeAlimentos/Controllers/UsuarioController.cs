using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using VacacionesBancodeAlimentos.Context;
using VacacionesBancodeAlimentos.Model;

namespace VacacionesBancodeAlimentos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _appContext;
        private readonly ViewsDbContext _viewsContext;
        private readonly IConfiguration _config;

        public UsuarioController(ApplicationDbContext appContext, ViewsDbContext viewsContext, IConfiguration config)
        {
            _appContext = appContext;
            _viewsContext = viewsContext;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var usuarios = await _appContext.Usuario.ToListAsync();
            var departamentosNominas = await _viewsContext.DepartamentosNominas.ToListAsync();

            var respuesta = usuarios.Select(u => new
            {
                u.IdUsuario,
                u.SSID,
                u.Nombre,
                u.Rol,
                IdDepartamento = u.Departamento,
                NombreDepartamento = departamentosNominas
                                        .FirstOrDefault(d => d.NumeroDepartamento == u.Departamento)?
                                        .Descripcion ?? "Sin Departamento"
            }).ToList();

            return Ok(respuesta);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UsuarioDto usuariodto)
        {
            if (usuariodto == null) return BadRequest("Los datos del usuario son nulos.");

            try
            {
                var usuario = new Usuario
                {
                    SSID = usuariodto.Sid,
                    Nombre = usuariodto.Nombre,
                    Departamento = 0,
                    Rol = 3
                };

                await _appContext.Usuario.AddAsync(usuario);
                await _appContext.SaveChangesAsync();

                return CreatedAtAction(nameof(Post), new { id = usuario.IdUsuario }, usuario);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "Error al guardar en la base de datos: " + ex.InnerException?.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.Token) || string.IsNullOrEmpty(loginDto.Nombre))
                return BadRequest("Token o nombre de usuario no proporcionados.");

            try
            {
                // 1. Configurar los parámetros de validación usando tu AppSettings
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateLifetime = true, // Aquí valida automáticamente la expiración
                    ClockSkew = TimeSpan.Zero // Elimina el margen de gracia de 5 min por defecto
                };

                // 2. Validar Token (Si falla, lanza excepción)
                var principal = tokenHandler.ValidateToken(loginDto.Token, validationParameters, out SecurityToken validatedToken);

                // 3. Extraer el nombre del Token para comparar con el del Body
                var nombreEnToken = principal.Identity?.Name;
                // O si lo guardaste en un claim específico: 
                // var nombreEnToken = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                if (nombreEnToken != loginDto.Nombre)
                {
                    return Unauthorized(new { mensaje = "El nombre del token no coincide con el usuario proporcionado." });
                }

                // 4. Buscar el usuario en TU base de datos de Vacaciones
                var usuarioEnBD = await _appContext.Usuario
                    .FirstOrDefaultAsync(u => u.Nombre == loginDto.Nombre);

                if (usuarioEnBD == null)
                {
                    return Unauthorized(new { mensaje = "El usuario está autenticado pero no tiene permisos en el sistema de Vacaciones." });
                }

                // 5. Sesión Creada Exitosamente
                // Retornamos los datos del usuario y el rol para que el Frontend sepa qué mostrar
                return Ok(new
                {
                    mensaje = "Sesión iniciada correctamente",
                    usuario = usuarioEnBD.Nombre,
                    rol = usuarioEnBD.Rol,
                    departamento = usuarioEnBD.Departamento,
                    token = loginDto.Token // El frontend usará este mismo para futuras peticiones
                });
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new { mensaje = "La sesión ha expirado. Por favor, inicie sesión de nuevo." });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { mensaje = "Token inválido o corrupto: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return BadRequest("El ID del usuario no coincide.");
            }

            var usuarioEnDb = await _appContext.Usuario.FindAsync(id);

            if (usuarioEnDb == null)
            {
                return NotFound($"No se encontró el usuario con ID {id}.");
            }

            _appContext.Entry(usuarioEnDb).CurrentValues.SetValues(usuario);

            try
            {
                await _appContext.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "Error al actualizar: " + ex.InnerException?.Message);
            }
        }
    }
}
