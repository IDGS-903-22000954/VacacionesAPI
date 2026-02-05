using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacacionesBancodeAlimentos.Model
{
    [Table("vacaciones")]
    public class Vacaciones
    {
        [Key]
        [Required]
        [Column("empleadoId")]
        public int IdEmpleado { get; set; }
        [Required]
        [Column("anio")]
        public int Anio { get; set; }
        [Required]
        [Column("diasTotales")]
        public int DiasTotales { get; set; }

        [ForeignKey("IdEmpleado")]
        public Empleado? Empleado { get; set; }
    }
}
