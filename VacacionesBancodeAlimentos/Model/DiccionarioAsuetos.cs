using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacacionesBancodeAlimentos.Model
{
    [Table("diccionarioFechas")]
    public class DiccionarioAsuetos
    {
        [Key]
        [Required]
        [Column("idFecha")]
        public int IdFecha { get; set; }
        [Required]
        [Column("nombre")]
        public string Nombre { get; set; }

        public ICollection<AsuetosFechas> AsuetosFechas { get; set; }
    }
}
