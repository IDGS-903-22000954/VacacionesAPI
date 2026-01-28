using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacacionesBancodeAlimentos.Model
{
    [Table("calculos")]
    public class Calculo
    {
        [Key]
        [Required]
        [Column("indice")]
        public int Indice { get; set; }
        [Required]
        [Column("anioMin")]
        public int AnioMin { get; set; }
        [Required]
        [Column("anioMax")]
        public int AnioMax { get; set; }
        [Required]
        [Column("dias")]
        public int Dias {  get; set; }
    }
}
