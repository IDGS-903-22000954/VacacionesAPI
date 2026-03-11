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
        public DbSet<DiccionarioAsuetos> DiccionarioAsuetos { get; set; }
        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<SolicitudFecha> SolicitudFechas { get; set; }
        public DbSet<Vacaciones> Vacaciones { get; set; }
        public DbSet<AsuetosFechas> AsuetosFechas { get; set; }
        public DbSet<AntiguedadEmpleados> AntiguedadEmpleados { get; set; }
        public DbSet<Pagina> Pagina { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Calculo>().ToTable("calculos");
            modelBuilder.Entity<DiccionarioAsuetos>().ToTable("diccionarioAsuetos");
            modelBuilder.Entity<Solicitud>().ToTable("solicitudes");
            modelBuilder.Entity<SolicitudFecha>().ToTable("solicitudesFechas");
            modelBuilder.Entity<Vacaciones>().ToTable("vacaciones").HasKey(v => new { v.IdEmpleado, v.Periodo });
            modelBuilder.Entity<AsuetosFechas>().ToTable("asuetosFechas");
            modelBuilder.Entity<AntiguedadEmpleados>().ToTable("antiguedadEmpleados");
            modelBuilder.Entity<Pagina>().ToTable("pagina");
        }
    }
}
