using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacacionesBancodeAlimentos.Model
{
    [Table("asuetosFechas")]
    public class AsuetosFechas
    {
        [Key]
        [Required]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("fechaId")]
        public int IdFecha { get; set; }
        [Required]
        [Column("fecha")]
        public DateOnly Fecha { get; set; }

        [ForeignKey("IdFecha")]
        public DiccionarioAsuetos? DiccionarioAsuetos { get; set; }
    }
}
