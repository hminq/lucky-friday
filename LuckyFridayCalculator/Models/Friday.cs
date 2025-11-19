using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LuckyFridayCalculator.Models.Enums;
using System.Linq;

namespace LuckyFridayCalculator.Models;

public class Friday
{
    public int Id { get; set; }
    
    public int AccountId { get; set; }
    public required virtual Account Account { get; set; }
    
    public DateTime BetDateTime { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal TotalOddsHongKong { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal TotalOddsInternational { get; set; }

    [Range(0.01, 8099999.99)]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalDeposit { get; set; }

    public BetStatus Status { get; set; }

    [MaxLength(100)]
    public string? Dog { get; set; }

    // Một Friday có MỘT lineup (gồm nhiều entry)
    public virtual ICollection<LineupEntry> LineupEntries { get; set; } = new List<LineupEntry>();

    // Một Friday có 3 trận single bet (direct relationship)
    public virtual ICollection<SingleBet> SingleBets { get; set; } = new List<SingleBet>();

    // Một Friday có thể có nhiều HedgeSet
    public virtual ICollection<HedgeSet> HedgeSets { get; set; } = new List<HedgeSet>();

    // Back-compat cho code cũ (lấy hedge set đầu tiên)
    [NotMapped]
    public HedgeSet? HedgeSet
    {
        get => HedgeSets.FirstOrDefault();
        set
        {
            HedgeSets.Clear();
            if (value != null)
            {
                HedgeSets.Add(value);
            }
        }
    }
}