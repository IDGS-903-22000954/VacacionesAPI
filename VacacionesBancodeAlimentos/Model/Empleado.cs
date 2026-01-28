using System.ComponentModel.DataAnnotations;

namespace VacacionesBancodeAlimentos.Model
{
    public class Empleado
    {
        [Key]
        public int IdEmpleado { get; set; }
        [StringLength(100)]
        public string Nombre { get; set; }
        [StringLength(30)]
        public string Puesto { get; set; }
        [StringLength(30)]
        public string Departamento { get; set; }
        public DateOnly FechaIngreso { get; set; }

        public ICollection<Vacaciones> Vacaciones { get; set; }
        public ICollection<Solicitud> Solicitud { get; set; }
    }
}
