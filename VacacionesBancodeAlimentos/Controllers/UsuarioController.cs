using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    // Ponemos estos en false para que no choquen con el token de la Intranet
                    // pero mantenemos la seguridad mediante la firma de la llave (Key)
                    ValidateIssuer = false,
                    ValidateAudience = false,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // 2. Validar Token y obtener la identidad
                var principal = tokenHandler.ValidateToken(loginDto.Token, validationParameters, out SecurityToken validatedToken);

                // 3. Extraer el nombre buscando el claim específico de tipo Name
                var nombreEnToken = principal.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Name || c.Type == "unique_name")?.Value;

                if (string.IsNullOrEmpty(nombreEnToken) || nombreEnToken != loginDto.Nombre)
                {
                    return Unauthorized(new { mensaje = "El nombre del token no coincide con el enviado." });
                }

                // 4. Buscar usuario en tabla por nombre
                var usuarioEnBD = await _appContext.Usuario
                    .FirstOrDefaultAsync(u => u.Nombre == loginDto.Nombre);

                if (usuarioEnBD == null)
                {
                    return Unauthorized(new { mensaje = "Usuario no registrado en el sistema de Vacaciones." });
                }

                // 5. Sesión Exitosa
                return Ok(new
                {
                    mensaje = "Sesión iniciada correctamente",
                    usuario = usuarioEnBD.Nombre,
                    rol = usuarioEnBD.Rol,
                    departamento = usuarioEnBD.Departamento,
                    token = loginDto.Token
                });
            }
            catch (Exception ex)
            {
                // Esto atrapará si el token está mal firmado o expirado
                return Unauthorized(new { mensaje = "Token inválido: " + ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Usuario usuario)
        {
            var userIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("SID")?.Value;

            if (string.IsNullOrEmpty(userIdToken))
            {
                return Unauthorized("No se pudo identificar al usuario de la sesión.");
            }

            if (id.ToString() == userIdToken)
            {
                return BadRequest("No tienes permitido modificar tu propia información de usuario.");
            }

            if (id != usuario.IdUsuario)
            {
                return BadRequest("El ID del usuario no coincide.");
            }

            var usuarioEnDb = await _appContext.Usuario.FindAsync(id);
            if (usuarioEnDb == null)
            {
                return NotFound($"No se encontró el usuario con ID {id}.");
            }

            // Actualización de valores
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

        // --- GENERADOR DE TOKEN FALSO ---
        [HttpGet("GenerarPase")]
        [AllowAnonymous]
        public IActionResult GenerarPase()
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "Cesar García"),
            new Claim("SID", "S-1-5-21-3623811015-3361044348-30300820-1013")
        };

            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes("BaLe0n-Intranet-2026-JWT-Key-Segura-01")
            );
            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "IntranetAPI",
                audience: "IntranetFrontend",
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds
            );

            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}