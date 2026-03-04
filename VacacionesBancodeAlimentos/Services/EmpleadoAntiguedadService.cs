using Dapper;
using Microsoft.Data.SqlClient;
using VacacionesBancodeAlimentos.Interfaces;
using VacacionesBancodeAlimentos.Model;

namespace VacacionesBancodeAlimentos.Services
{
    public class EmpleadoAntiguedadService : IEmpleadoAntiguedadService
    {
        private readonly IConfiguration _config;

        public EmpleadoAntiguedadService(IConfiguration config)
        {
            _config = config;
        }

        //Script para generar fechas del contrato de forma aleatoria, unicamente para pruebas
        public async Task GenerarFechasPruebaAsync()
        {
            // Cadenas de conexión desde appsettings.json
            var connA = _config.GetConnectionString("ctBANOM"); 
            var connB = _config.GetConnectionString("BDVacaciones");

            using var dbA = new SqlConnection(connA);
            using var dbB = new SqlConnection(connB);

            // 1. Traemos los IDs del Servidor A
            var ids = await dbA.QueryAsync<string>("SELECT codigoempleado FROM VW_EmpleadosNominas");

            var rnd = new Random();
            var datos = ids.Select(id => new {
                Id = id,
                Fecha = DateTime.Today.AddDays(-rnd.Next(365, 1825)), // Entre 1 y 5 años atrás
                Antiguedad = 0
            }).ToList();

            // 2. Insertamos en el Servidor B
            string sql = "INSERT INTO antiguedadEmpleados (EmpleadoId, fechaContrato, antiguedad) VALUES (@Id, @Fecha, @Antiguedad)";
            await dbB.ExecuteAsync(sql, datos);
        }

        //Script para validar la antiguedad de todos los empleados y actualizarla 
        public async Task ActualizarAntiguedadGeneralAsync()
        {
            var conn = _config.GetConnectionString("BDVacaciones");
            using var db = new SqlConnection(conn);

            // Obtenemos los empleados
            var listaAntiguedades = await db.QueryAsync<AntiguedadEmpleados>("SELECT * FROM antiguedadEmpleados");
            var matriz = await db.QueryAsync<Calculo>("SELECT * FROM calculos");
            var fechaHoy = DateTime.Today;

            foreach (var registro in listaAntiguedades)
            {
                // Cálculo preciso de años (Lógica de Aniversario)
                int antiguedadCalculada = fechaHoy.Year - registro.FechaContrato.Year;

                // Si la fecha de contrato ajustada al año actual es mayor a HOY, 
                // significa que aún no cumple el año en el calendario actual.
                if (registro.FechaContrato.Date > fechaHoy.AddYears(-antiguedadCalculada))
                {
                    antiguedadCalculada--;
                }

                var rangoEncontrado = matriz.FirstOrDefault(m =>
                    antiguedadCalculada >= m.AnioMin &&
                    antiguedadCalculada <= m.AnioMax);
                int diasAsignados = rangoEncontrado?.Dias ?? 0;

                // Validación y Actualización
                if (registro.Antiguedad != antiguedadCalculada)
                {
                    string sqlUpdate = @"UPDATE antiguedadEmpleados SET Antiguedad = @nuevaAntiguedad WHERE empleadoId = @id";
                    await db.ExecuteAsync(sqlUpdate, new
                    {
                        nuevaAntiguedad = antiguedadCalculada,
                        id = registro.EmpleadoId
                    });

                    string sqlVacacion = @"INSERT INTO vacaciones (empleadoId, periodo, diasTotales, diasRestantes) VALUES (@id, @periodo, @dias, @dias)";
                    await db.ExecuteAsync(sqlVacacion, new
                    {
                        id = registro.EmpleadoId,
                        periodo = fechaHoy.Year + 1,
                        dias = diasAsignados
                    });

                    Console.WriteLine($"Empleado {registro.EmpleadoId} actualizado a {antiguedadCalculada} años.");
                }
            }
        }
    }
}
