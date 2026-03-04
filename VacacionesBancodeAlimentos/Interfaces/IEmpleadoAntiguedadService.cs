namespace VacacionesBancodeAlimentos.Interfaces
{
    public interface IEmpleadoAntiguedadService
    {
        Task GenerarFechasPruebaAsync();
        Task ActualizarAntiguedadGeneralAsync();
    }
}
