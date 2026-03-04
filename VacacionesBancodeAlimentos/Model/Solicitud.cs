using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacacionesBancodeAlimentos.Model
{
    [Table("solicitudes")]
    public class Solicitud
    {
        [Key]
        [Required]
        [Column("idSolicitud")]
        public int IdSolicitud { get; set; }
        [Required]
        [Column("empleadoId")]
        public string IdEmpleado { get; set; }
        [Column("formato")]
        public string Formato { get; set; }
        [Required]
        [Column("estatus")]
        public char Estatus { get; set; }
        [Required]
        [Column("fechaPeticion")]
        public DateTime FechaPeticion {  get; set; }

        [ForeignKey("IdEmpleado")]
        public V_EmpleadosNominas? Empleado { get; set; }

        public ICollection<SolicitudFecha> SolicitudFechas { get; set; }

    }
}
