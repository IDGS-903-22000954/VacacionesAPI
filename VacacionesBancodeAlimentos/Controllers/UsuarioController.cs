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

        [Authorize+]
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
        [Authorize]
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Validación de entrada
            if (string.IsNullOrEmpty(loginDto.Token) || string.IsNullOrEmpty(loginDto.SID))
                return BadRequest(new { mensaje = "Token o SID de usuario no proporcionados." });

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Valida la firma y vigencia del token
                var principal = tokenHandler.ValidateToken(loginDto.Token, validationParameters, out SecurityToken validatedToken);

                // 1. Extraer el SID del Token decodificado
                var sidEnToken = principal.Claims.FirstOrDefault(c =>
                    c.Type == "SID" ||
                    c.Type == ClaimTypes.Sid ||
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid")?.Value;

                // 2. Validar consistencia (Que el SID del JSON coincida con el del Token)
                if (string.IsNullOrEmpty(sidEnToken) || sidEnToken != loginDto.SID)
                {
                    return Unauthorized(new { mensaje = "La identidad del usuario no coincide con el token enviado." });
                }

                // 3. Buscar usuario en la DB de Vacaciones usando el SID
                // NOTA: Asegúrate que tu entidad Usuario tenga la columna SSID o SID
                var usuarioEnBD = await _appContext.Usuario
                    .FirstOrDefaultAsync(u => u.SSID == loginDto.SID);

                if (usuarioEnBD == null)
                {
                    return Unauthorized(new { mensaje = "Acceso denegado: El usuario no está registrado en el sistema de Vacaciones." });
                }

                // 4. Respuesta exitosa
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
                return Unauthorized(new { mensaje = "Error de validación: " + ex.Message });
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
            new Claim(ClaimTypes.Name, "Carlos Bodridoza"),
            new Claim("SID", "S-1-5-21-3623811015-3361044348-30300820-1015")
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