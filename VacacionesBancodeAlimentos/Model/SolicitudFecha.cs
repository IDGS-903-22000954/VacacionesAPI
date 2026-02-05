using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacacionesBancodeAlimentos.Model
{
    [Table("solicitudesFechas")]
    public class SolicitudFecha
    {
        [Key]
        [Required]
        [Column("solicitudId")]
        public int IdSolicitud {  get; set; }
        [Required]
        [Column("fecha")]
        public DateOnly Fecha { get; set; }

        [ForeignKey("IdSolicitud")]
        public Solicitud? Solicitud { get; set; }
    }
}
