using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacacionesBancodeAlimentos.Model
{
    [Table("pagina")]
    public class Pagina
    {
        [Key]
        [Required]
        [Column("idPagina")]
        public int IdPagina { get; set; }
        [Required]
        [Column("ruta")]
        public string Ruta { get; set; }
    }
}
