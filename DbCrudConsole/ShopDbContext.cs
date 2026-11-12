using Microsoft.EntityFrameworkCore;

namespace DbCrudConsole;

public class ShopDbContext : DbContext
{
    private readonly string _connectionString;

    public ShopDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbSet<Game> Games => Set<Game>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(entity =>
        {
            entity.ToTable("Games");
            entity.HasKey(g => g.GameID);

            entity.Property(g => g.GameID).HasColumnName("GameID").ValueGeneratedOnAdd();
            entity.Property(g => g.Title).HasColumnName("Title").IsRequired().HasMaxLength(150);
            entity.Property(g => g.Description).HasColumnName("Description");
            entity.Property(g => g.Price).HasColumnName("Price").HasPrecision(10, 2);
            entity.Property(g => g.ReleaseDate).HasColumnName("ReleaseDate");
            entity.Property(g => g.IsAvailable).HasColumnName("IsAvailable");
        });
    }
}