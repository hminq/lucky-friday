using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LuckyFridayCalculator.Models.Enums;

namespace LuckyFridayCalculator.Models;

public class SingleBet
{
    public int Id { get; set; }
    
    [Required, MaxLength(250)]
    public required string Title { get; set; }
    
    public DateTime MatchStartTime { get; set; }
    
    public DateTime MatchEndTime { get; set; }
    
    [Column(TypeName = "decimal(18, 4)")]
    public decimal OddsHongKong { get; set; }
    
    [Column(TypeName = "decimal(18, 4)")]
    public decimal OddsInternational { get; set; }
    
    public BetStatus Status { get; set; }
    
    // SingleBet thuộc về Friday (direct relationship)
    public int? FridayId { get; set; }
    public virtual Friday? Friday { get; set; }
    
    // SingleBet có thể thuộc về HedgeSet
    public int? HedgeSetId { get; set; }
    public virtual HedgeSet? HedgeSet { get; set; }
}