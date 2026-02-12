using System.ComponentModel.DataAnnotations;

namespace VacacionesBancodeAlimentos.Model
{
    public class V_EmpleadosNominas
    {
        [Key]
        public int CodigoEmpleado {  get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public int IdPuesto { get; set; }
        public string Puesto { get; set; }
        public int IdDepartamento { get; set; }
        public string Departamento { get; set; }
        public DateOnly FechaAlta { get; set; }
        public DateOnly FechaReingreso { get; set; }
        public string TipoContrato { get; set; }

    }
}
