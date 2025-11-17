using System.ComponentModel.DataAnnotations.Schema;

namespace LuckyFridayCalculator.Models;

public class LineupEntry
{
    public int Id { get; set; }
    
    public int MemberId { get; set; }
    public required virtual Member Member { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }
    public int FridayId { get; set; }
    public required virtual Friday Friday { get; set; }
}