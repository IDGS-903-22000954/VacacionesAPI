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
        public string IdEmpleado { get; set; }
        [Required]
        [Column("periodo")]
        public int Periodo { get; set; }
        [Required]
        [Column("diasTotales")]
        public int DiasTotales { get; set; }
        [Required]
        [Column("diasRestantes")]
        public int DiasRestantes { get; set; }
    }
}
