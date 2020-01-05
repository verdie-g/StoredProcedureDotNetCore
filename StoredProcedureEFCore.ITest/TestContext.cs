using Microsoft.EntityFrameworkCore;

namespace StoredProcedureEFCore.ITest
{
  public partial class TestContext : DbContext
  {
    public TestContext(DbContextOptions<TestContext> options)
      : base(options)
    {
    }

    public virtual DbSet<Table1> Table1 { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Table1>(entity =>
      {
        entity.ToTable("table_1");

        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.Active).HasColumnName("active");

        entity.Property(e => e.Date)
                  .HasColumnName("date")
                  .HasColumnType("datetime");

        entity.Property(e => e.Name)
                  .HasColumnName("name")
                  .HasMaxLength(255)
                  .IsUnicode(false);

        entity.Property(e => e.NameWithUnderscore)
                  .HasColumnName("name_with_underscore")
                  .HasDefaultValueSql("((0))");
      });
    }
  }
}
