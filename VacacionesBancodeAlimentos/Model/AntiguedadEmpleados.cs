using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacacionesBancodeAlimentos.Model
{
    [Table("antiguedadEmpleados")]
    public class AntiguedadEmpleados
    {
        [Key]
        [Required]
        [Column("empleadoId")]
        public string EmpleadoId { get; set; }
        [Required]
        [Column("fechaContrato")]
        public DateTime FechaContrato {  get; set; }
        [Required]
        [Column("antiguedad")]
        public int Antiguedad {  get; set; }
    }
}
