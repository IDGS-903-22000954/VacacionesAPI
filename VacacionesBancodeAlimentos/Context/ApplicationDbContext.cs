using Microsoft.EntityFrameworkCore;
using VacacionesBancodeAlimentos.Model;

namespace VacacionesBancodeAlimentos.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Calculo> Calculos { get; set; }
        public DbSet<DiccionarioFechas> DiccionarioFechas { get; set; }
        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<SolicitudFecha> SolicitudFechas { get; set; }
        public DbSet<Vacaciones> Vacaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Calculo>().ToTable("calculos");
            modelBuilder.Entity<DiccionarioFechas>().ToTable("diccionarioFechas");
            modelBuilder.Entity<Solicitud>().ToTable("solicitudes");
            modelBuilder.Entity<SolicitudFecha>().ToTable("solicitudesFechas");
            modelBuilder.Entity<Vacaciones>().ToTable("vacaciones");
        }
    }
}
