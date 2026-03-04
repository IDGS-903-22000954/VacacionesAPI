using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace VacacionesBancodeAlimentos.Model
{
    [Table("solicitudesFechas")]
    public class SolicitudFecha
    {
        [Key]
        [Required]
        [Column("idFecha")]
        public int IdFecha {  get; set; }
        [Required]
        [Column("solicitudId")]
        public int SolicitudId { get; set; }
        [Required]
        [Column("fecha")]
        public DateTime Fecha { get; set; }
        [JsonIgnore]

        [ForeignKey("SolicitudId")]
        public Solicitud? Solicitud { get; set; }
    }
}
