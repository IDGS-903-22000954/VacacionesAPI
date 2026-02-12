using System.ComponentModel.DataAnnotations;

namespace VacacionesBancodeAlimentos.Model
{
    public class V_PuestosNominas
    {
        [Key]
        public int IdPuesto { get; set; }
        public int NumeroPuesto { get; set; }
        public string Descripcion { get; set; }
    }
}
