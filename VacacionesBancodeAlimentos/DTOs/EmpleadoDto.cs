public class EmpleadoDetalleDto
{
    public string CodigoEmpleado { get; set; }
    public string NombreCompleto { get; set; }
    public string Puesto { get; set; }
    public string Departamento { get; set; }
    public DateTime FechaContrato { get; set; }
    public int Antiguedad { get; set; }
    // Aquí anidamos la lista de vacaciones
    public List<VacacionesDto> PeriodosVacaciones { get; set; }
    public DateTime? FechaPeticion { get; set; }
}

public class VacacionesDto
{
    public int Periodo { get; set; }
    public int DiasTotales { get; set; }
    public int DiasRestantes { get; set; }
}
