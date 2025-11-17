using Microsoft.EntityFrameworkCore;

namespace LuckyFridayCalculator.Models;

public class LuckyFridayDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<SingleBet> SingleBets { get; set; }
    public DbSet<Friday> Fridays { get; set; }
    public DbSet<LineupEntry> LineupEntries { get; set; }
    public DbSet<HedgeSet> HedgeSets { get; set; }

    public LuckyFridayDbContext(DbContextOptions<LuckyFridayDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Friday - SingleBet: One-to-Many (1 Friday có 3 SingleBet)
        modelBuilder.Entity<SingleBet>()
            .HasOne(sb => sb.Friday)
            .WithMany(f => f.SingleBets)
            .HasForeignKey(sb => sb.FridayId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        modelBuilder.Entity<LineupEntry>()
            .HasOne(le => le.Member)
            .WithMany(m => m.LineupEntries)
            .HasForeignKey(le => le.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LineupEntry>()
            .HasOne(le => le.Friday)
            .WithMany(f => f.LineupEntries)
            .HasForeignKey(le => le.FridayId)
            .OnDelete(DeleteBehavior.Cascade);

        // Friday - HedgeSet: 1-1 relationship
        modelBuilder.Entity<Friday>()
            .HasOne(f => f.HedgeSet)
            .WithOne(hs => hs.Friday)
            .HasForeignKey<HedgeSet>(hs => hs.FridayId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // SingleBet - HedgeSet: Many-to-One (1 HedgeSet có 2 SingleBet)
        // Dùng NoAction để tránh multiple cascade paths (Friday -> HedgeSet -> SingleBet)
        modelBuilder.Entity<SingleBet>()
            .HasOne(sb => sb.HedgeSet)
            .WithMany(hs => hs.SingleBets)
            .HasForeignKey(sb => sb.HedgeSetId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        modelBuilder.Entity<Friday>()
            .ToTable(b => b.HasCheckConstraint("CK_Friday_TotalDeposit", "[TotalDeposit] > 0 AND [TotalDeposit] < 8100000"));

        modelBuilder.Entity<Account>().HasData(
            new Account { Id = 1, Title = "Account 1" },
            new Account { Id = 2, Title = "Account 2" }
        );
    }
}
