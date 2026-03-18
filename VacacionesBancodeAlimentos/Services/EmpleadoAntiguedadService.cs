using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
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

        public async Task ActualizarAntiguedadGeneralAsync()
        {
            var connString = _config.GetConnectionString("BDVacaciones");
            using var db = new SqlConnection(connString);
            await db.OpenAsync();

            var empleadosIds = await db.QueryAsync<string>("SELECT empleadoId FROM antiguedadEmpleados");

            foreach (var id in empleadosIds)
            {
                using var transaction = db.BeginTransaction();
                try
                {
                    await ActualizarAntiguedadPorEmpleadoAsync(id, transaction);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error en empleado {id}: {ex.Message}");
                }
            }
        }

        // MÉTODO INDIVIDUAL: La "Única Fuente de Verdad"
        public async Task ActualizarAntiguedadPorEmpleadoAsync(string id, IDbTransaction transaction)
        {
            var db = transaction.Connection;
            var fechaHoy = DateTime.Today;

            var registro = await db.QueryFirstOrDefaultAsync<AntiguedadEmpleados>(
                "SELECT * FROM antiguedadEmpleados WHERE empleadoId = @id", new { id }, transaction);

            var matriz = await db.QueryAsync<Calculo>("SELECT * FROM calculos", transaction: transaction);

            if (registro == null) return;

            // Cálculo de aniversario
            int antiguedadCalculada = fechaHoy.Year - registro.FechaContrato.Year;
            if (registro.FechaContrato.Date > fechaHoy.AddYears(-antiguedadCalculada))
                antiguedadCalculada--;

            var rango = matriz.FirstOrDefault(m => antiguedadCalculada >= m.AnioMin && antiguedadCalculada <= m.AnioMax);
            int diasNuevos = rango?.Dias ?? 0;

            // El periodo es el año en que se genera el derecho (Aniversario)
            int periodoActual = registro.FechaContrato.AddYears(antiguedadCalculada).Year;

            // 1. Sincronizar tabla maestra
            await db.ExecuteAsync(
                "UPDATE antiguedadEmpleados SET Antiguedad = @ant WHERE empleadoId = @id",
                new { ant = antiguedadCalculada, id }, transaction);

            // 2. UPSERT Robusto en Vacaciones
            // Explicación: Calculamos el uso actual (Totales - Restantes) y lo restamos al nuevo total.
            // Usamos CASE para asegurar que diasRestantes nunca sea menor a 0 (por seguridad contable).
            string sqlUpsert = @"
            IF EXISTS (SELECT 1 FROM vacaciones WHERE empleadoId = @id AND periodo = @periodo)
            BEGIN
                UPDATE vacaciones 
                SET 
                    diasRestantes = CASE 
                        WHEN (@dias - (diasTotales - diasRestantes)) < 0 THEN 0 
                        ELSE (@dias - (diasTotales - diasRestantes)) 
                    END,
                    diasTotales = @dias
                WHERE empleadoId = @id AND periodo = @periodo
            END
            ELSE
            BEGIN
                INSERT INTO vacaciones (empleadoId, periodo, diasTotales, diasRestantes, diasDevueltos)
                VALUES (@id, @periodo, @dias, @dias, 0)
            END";

            await db.ExecuteAsync(sqlUpsert, new
            {
                id,
                periodo = periodoActual,
                dias = diasNuevos
            }, transaction);
        }
    }
}
