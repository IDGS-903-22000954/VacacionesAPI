using System.ComponentModel.DataAnnotations;

namespace VacacionesBancodeAlimentos.Model
{
    public class V_DepartamentosNominas
    {
        [Key]
        public int IdDepartamento {  get; set; }
        public int NumeroDepartamento { get; set; }
        public string Descripcion { get; set; }
        public string BaseOrigen {  get; set; }
    }
}
