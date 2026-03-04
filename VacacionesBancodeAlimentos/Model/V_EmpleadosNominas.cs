using System.ComponentModel.DataAnnotations;

namespace VacacionesBancodeAlimentos.Model
{
    public class V_EmpleadosNominas
    {
        [Key]
        public string CodigoEmpleado {  get; set; }
        public string Nombre { get; set; }  
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public int IdPuesto { get; set; }
        public string Puesto { get; set; }
        public int IdDepartamento { get; set; }
        public string Dpto { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime FechaReingreso { get; set; }
        public string TipoContrato { get; set; }

    }
}
