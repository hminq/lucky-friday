using System.ComponentModel.DataAnnotations;

namespace LuckyFridayCalculator.Models;

public class Account
{
    public int Id { get; set; } 
    [Required]
    [MaxLength(100)]
    public required string Title { get; set; }
    public virtual ICollection<Friday> Fridays { get; set; } = new List<Friday>();
}
