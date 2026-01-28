using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacacionesBancodeAlimentos.Model
{
    [Table("diccionarioFechas")]
    public class DiccionarioFechas
    {
        [Key]
        [Required]
        [Column("idFecha")]
        public int IdFecha { get; set; }
        [Required]
        [Column("nombre")]
        public string Nombre { get; set; }
        [Required]
        [Column("fecha")]
        public DateOnly Fecha { get; set; }
    }
}
