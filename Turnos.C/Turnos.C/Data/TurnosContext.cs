using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Turnos.C.Models;

namespace Turnos.C.Data
{
    public class TurnosContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
    {
        public TurnosContext(DbContextOptions options)
            : base(options)
        { 
        

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Prestacion>()
                .Property(prestacion => prestacion.Precio)
                .HasPrecision(38, 18);

            modelBuilder.Entity<Turno>()
                .HasOne(t => t.Paciente)
                .WithMany(p => p.Turnos)
                .HasForeignKey(t => t.PacienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Turno>()
                .HasOne(t => t.Profesional)
                .WithMany(p => p.Turnos)
                .HasForeignKey(t => t.ProfesionalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Profesional>()
                .HasOne(p => p.Matricula)
                .WithOne(m => m.Profesional)
                .HasForeignKey<Matricula>(m => m.ProfesionalId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cobertura>()
                .HasIndex(c => c.NumeroCredencial)
                .IsUnique();

            modelBuilder.Entity<Persona>()
                .HasIndex(p => p.DNI)
                .IsUnique();

            modelBuilder.Entity<Matricula>()
                .HasIndex(m => m.NumeroMatricula)
                .IsUnique();

            modelBuilder.Entity<Prestacion>()
                .HasIndex(p => p.Nombre)
                .IsUnique();

            // Para usar tabla Personas en vez de ASPNETUSERS
            modelBuilder.Entity<IdentityUser<int>>().ToTable("Personas");
            // Para usar tabla Roles en vez de ASPNETROLES
            modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles");
            // Para usar tabla PersonasRoles en vez de 
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("PersonasRoles");

        }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Profesional> Profesionales { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Prestacion> Prestaciones { get; set; }
        public DbSet<Telefono> Telefonos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }
        public DbSet<Cobertura> Coberturas { get; set; }
        public DbSet<Direccion> Direcciones { get; set; }
        public DbSet<Formulario> Formularios { get; set; }    
        public DbSet<Rol> Roles { get; set; }
        
    }
}
