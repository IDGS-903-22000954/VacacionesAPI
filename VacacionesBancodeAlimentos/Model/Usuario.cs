using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacacionesBancodeAlimentos.Model
{
    [Table("usuario")]
    public class Usuario
    {
        [Key]
        [Required]
        [Column("idUsuario")]
        public int IdUsuario { get; set; }
        [Required]
        [Column("ssid")]
        public string SSID { get; set; }
        [Required]
        [Column("nombre")]
        public string Nombre { get; set; }
        [Required]
        [Column("rol")]
        public int Rol { get; set; }
        [Required]
        [Column("Departamento")]
        public int Departamento { get; set; }
    }
}
