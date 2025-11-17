using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuckyFridayCalculator.Models;


public class HedgeSet
{
    public int Id { get; set; }

    public int FridayId { get; set; }
    public required virtual Friday Friday { get; set; }

    [Required, MaxLength(200)]
    public required string Title { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Budget { get; set; }

    public virtual ICollection<SingleBet> SingleBets { get; set; } = new List<SingleBet>();
}