using EntryLog.Entities.POCOEntities;
using Microsoft.EntityFrameworkCore;

namespace EntryLog.Data.SqlLegacy.Contexts;

internal class EmployeesDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(builder =>
        {
            builder.ToTable("EMPLEADOS");
            builder.HasKey(x => x.Code);

            builder.Property(x => x.Code).HasColumnName("CODIGO");
            builder.Property(x => x.FullName).HasColumnName("NOMBRES");
            builder.Property(x => x.Position).HasColumnName("CARGOS");
            builder.Property(x => x.OrganizationID).HasColumnName("EMPRESA");
            builder.Property(x => x.BranchOffice).HasColumnName("NOMBRE_SUC");
            builder.Property(x => x.TownName).HasColumnName("CIUDAD");
            builder.Property(x => x.CostCenter).HasColumnName("CENTRO_DE_COSTO");
        });

        base.OnModelCreating(modelBuilder);
    }


}
