using System.Data;

namespace VacacionesBancodeAlimentos.Interfaces
{
    public interface IEmpleadoAntiguedadService
    {
        Task ActualizarAntiguedadGeneralAsync();
        Task ActualizarAntiguedadPorEmpleadoAsync(string id, IDbTransaction transaction);
    }
}
