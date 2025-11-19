using System.ComponentModel.DataAnnotations.Schema;

namespace LuckyFridayCalculator.Models;

public class HedgeSetLineupEntry
{
    public int Id { get; set; }

    public int HedgeSetId { get; set; }
    public required virtual HedgeSet HedgeSet { get; set; }

    public int MemberId { get; set; }
    public required virtual Member Member { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }
}
