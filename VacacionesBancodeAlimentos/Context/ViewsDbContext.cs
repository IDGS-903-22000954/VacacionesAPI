using Microsoft.EntityFrameworkCore;
using VacacionesBancodeAlimentos.Model;

namespace VacacionesBancodeAlimentos.Context
{
    public class ViewsDbContext : DbContext
    {
        public ViewsDbContext(DbContextOptions<ViewsDbContext> options) : base(options)
        {
        }

        public DbSet<V_DepartamentosNominas> DepartamentosNominas { get; set; }
        public DbSet<V_EmpleadosNominas> EmpleadosNominas { get; set; }
        public DbSet<V_PuestosNominas> PuestosNominas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<V_DepartamentosNominas>().ToView("VW_DepartamentosNominas");
            modelBuilder.Entity<V_EmpleadosNominas>().ToView("VW_EmpleadosNominas");
            modelBuilder.Entity<V_PuestosNominas>().ToView("VW_PuestosNominas");
        }
    }
}