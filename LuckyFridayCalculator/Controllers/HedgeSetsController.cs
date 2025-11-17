using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuckyFridayCalculator.Models;

namespace LuckyFridayCalculator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HedgeSetsController : ControllerBase
{
    private readonly LuckyFridayDbContext _context;

    public HedgeSetsController(LuckyFridayDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HedgeSetDto>>> GetHedgeSets()
    {
        var hedgeSets = await _context.HedgeSets
            .Include(hs => hs.Friday)
                .ThenInclude(f => f.Account)
            .Include(hs => hs.Friday)
                .ThenInclude(f => f.LineupEntries)
                    .ThenInclude(le => le.Member)
            .Include(hs => hs.SingleBets)
            .OrderByDescending(hs => hs.Friday.BetDateTime)
            .ToListAsync();

        return hedgeSets.Select(hs => new HedgeSetDto
        {
            Id = hs.Id,
            FridayId = hs.FridayId,
            FridayDate = hs.Friday.BetDateTime,
            FridayAccountTitle = hs.Friday.Account.Title,
            Title = hs.Title,
            Budget = hs.Budget,
            SingleBetsCount = hs.SingleBets.Count,
            LineupEntries = hs.Friday.LineupEntries.Select(le => new HedgeSetLineupEntryDto
            {
                Id = le.Id,
                MemberId = le.MemberId,
                MemberName = le.Member.Name,
                Amount = le.Amount
            }).ToList()
        }).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HedgeSetDto>> GetHedgeSet(int id)
    {
        var hedgeSet = await _context.HedgeSets
            .Include(hs => hs.Friday)
                .ThenInclude(f => f.Account)
            .Include(hs => hs.Friday)
                .ThenInclude(f => f.LineupEntries)
                    .ThenInclude(le => le.Member)
            .Include(hs => hs.SingleBets)
            .FirstOrDefaultAsync(hs => hs.Id == id);

        if (hedgeSet == null)
        {
            return NotFound();
        }

        return new HedgeSetDto
        {
            Id = hedgeSet.Id,
            FridayId = hedgeSet.FridayId,
            FridayDate = hedgeSet.Friday.BetDateTime,
            FridayAccountTitle = hedgeSet.Friday.Account.Title,
            Title = hedgeSet.Title,
            Budget = hedgeSet.Budget,
            SingleBetsCount = hedgeSet.SingleBets.Count,
            LineupEntries = hedgeSet.Friday.LineupEntries.Select(le => new HedgeSetLineupEntryDto
            {
                Id = le.Id,
                MemberId = le.MemberId,
                MemberName = le.Member.Name,
                Amount = le.Amount
            }).ToList()
        };
    }
}

public class HedgeSetDto
{
    public int Id { get; set; }
    public int FridayId { get; set; }
    public DateTime FridayDate { get; set; }
    public string FridayAccountTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public int SingleBetsCount { get; set; }
    public List<HedgeSetLineupEntryDto> LineupEntries { get; set; } = new();
}

public class HedgeSetLineupEntryDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

