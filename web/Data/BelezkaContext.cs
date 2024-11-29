using web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace web.Data;
public class BelezkaContext : IdentityDbContext<ApplicationUser>

{
    public BelezkaContext(DbContextOptions<BelezkaContext> options) : base(options)
    {

    }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Nastavitve> Nastavitves { get; set; } //POR FAVOR
    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<PortfolioPerformance> PortfolioPerformances { get; set; }
    public DbSet<Transakcija> Transakcijas { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Watchlist> Watchlists { get; set; }
    public DbSet<WatchlistAsset> WatchlistAssets { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Asset>().ToTable("Asset");
        modelBuilder.Entity<Nastavitve>().ToTable("Nastavitve");
        modelBuilder.Entity<Portfolio>().ToTable("Portfolio");
        modelBuilder.Entity<PortfolioPerformance>().ToTable("PortfolioPerformance");
        modelBuilder.Entity<Transakcija>().ToTable("Transakcija");
        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<Watchlist>().ToTable("Watchlist");
        modelBuilder.Entity<WatchlistAsset>().ToTable("WatchlistAsset");
    }
}
