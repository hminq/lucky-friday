using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LuckyFridayCalculator.Models;
using LuckyFridayCalculator.Models.Enums;

namespace LuckyFridayCalculator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FridaysController : ControllerBase
{
    private readonly LuckyFridayDbContext _context;
    private readonly ILogger<FridaysController> _logger;

    public FridaysController(LuckyFridayDbContext context, ILogger<FridaysController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FridayDto>>> GetFridays()
    {
        var fridays = await _context.Fridays
            .Include(f => f.Account)
            .Include(f => f.LineupEntries)
                .ThenInclude(le => le.Member)
            .Include(f => f.SingleBets)
            .Include(f => f.HedgeSet)
            .OrderByDescending(f => f.BetDateTime)
            .ToListAsync();

        var today = DateTime.UtcNow.AddHours(7).Date;

        return fridays.Select(f => new FridayDto
        {
            Id = f.Id,
            AccountId = f.AccountId,
            AccountTitle = f.Account.Title,
            BetDateTime = f.BetDateTime,
            TotalOddsHongKong = f.TotalOddsHongKong,
            TotalOddsInternational = f.TotalOddsInternational,
            TotalDeposit = f.TotalDeposit,
            Status = f.Status,
            Dog = f.Dog,
            IsCurrentFriday = f.BetDateTime.Date == today,
            HasHedgeSet = f.HedgeSet != null,
            LineupEntries = f.LineupEntries.Select(le => new LineupEntryDto
            {
                Id = le.Id,
                MemberId = le.MemberId,
                MemberName = le.Member.Name,
                Amount = le.Amount
            }).ToList(),
            SingleBets = f.SingleBets.Where(sb => sb.FridayId == f.Id).Select(sb => new SingleBetDto
            {
                Id = sb.Id,
                Title = sb.Title,
                MatchStartTime = sb.MatchStartTime,
                MatchEndTime = sb.MatchEndTime,
                OddsHongKong = sb.OddsHongKong,
                OddsInternational = sb.OddsInternational,
                Status = sb.Status
            }).ToList()
        }).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FridayDto>> GetFriday(int id)
    {
        var friday = await _context.Fridays
            .Include(f => f.Account)
            .Include(f => f.LineupEntries)
                .ThenInclude(le => le.Member)
            .Include(f => f.SingleBets)
            .Include(f => f.HedgeSet)
                .ThenInclude(hs => hs != null ? hs.SingleBets : null!)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (friday == null)
        {
            return NotFound();
        }

        var today = DateTime.UtcNow.AddHours(7).Date;

        return new FridayDto
        {
            Id = friday.Id,
            AccountId = friday.AccountId,
            AccountTitle = friday.Account.Title,
            BetDateTime = friday.BetDateTime,
            TotalOddsHongKong = friday.TotalOddsHongKong,
            TotalOddsInternational = friday.TotalOddsInternational,
            TotalDeposit = friday.TotalDeposit,
            Status = friday.Status,
            Dog = friday.Dog,
            IsCurrentFriday = friday.BetDateTime.Date == today,
            HasHedgeSet = friday.HedgeSet != null,
            LineupEntries = friday.LineupEntries.Select(le => new LineupEntryDto
            {
                Id = le.Id,
                MemberId = le.MemberId,
                MemberName = le.Member.Name,
                Amount = le.Amount
            }).ToList(),
            SingleBets = friday.SingleBets.Where(sb => sb.FridayId == friday.Id).Select(sb => new SingleBetDto
            {
                Id = sb.Id,
                Title = sb.Title,
                MatchStartTime = sb.MatchStartTime,
                MatchEndTime = sb.MatchEndTime,
                OddsHongKong = sb.OddsHongKong,
                OddsInternational = sb.OddsInternational,
                Status = sb.Status
            }).ToList(),
            HedgeSet = friday.HedgeSet != null ? new FridayHedgeSetDto
            {
                Id = friday.HedgeSet.Id,
                Title = friday.HedgeSet.Title,
                Budget = friday.HedgeSet.Budget,
                SingleBets = friday.HedgeSet.SingleBets.Select(sb => new SingleBetDto
                {
                    Id = sb.Id,
                    Title = sb.Title,
                    MatchStartTime = sb.MatchStartTime,
                    MatchEndTime = sb.MatchEndTime,
                    OddsHongKong = sb.OddsHongKong,
                    OddsInternational = sb.OddsInternational,
                    Status = sb.Status
                }).ToList()
            } : null
        };
    }

    [HttpPost]
    public async Task<ActionResult<FridayDto>> CreateFriday([FromBody] CreateFridayDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = "Invalid model state", details = ModelState });
        }

        if (dto.AccountId <= 0)
        {
            return BadRequest(new { error = "AccountId is required" });
        }

        var account = await _context.Accounts.FindAsync(dto.AccountId);
        if (account == null)
        {
            return BadRequest(new { error = "Account not found" });
        }

        if (dto.TotalDeposit < 0.01m || dto.TotalDeposit >= 8100000m)
        {
            return BadRequest(new { error = "TotalDeposit must be between 0.01 and 8,099,999.99" });
        }

        // Validate lineup entries
        if (dto.LineupEntries == null || dto.LineupEntries.Count == 0)
        {
            return BadRequest(new { error = "Lineup is required. At least one member must be added." });
        }

        var lineupTotal = dto.LineupEntries.Sum(e => e.Amount);
        if (Math.Abs(lineupTotal - dto.TotalDeposit) >= 0.01m)
        {
            return BadRequest(new { error = $"Lineup total ({lineupTotal:N2}) must equal Total Deposit ({dto.TotalDeposit:N2})" });
        }

        // Validate all members exist
        var memberIds = dto.LineupEntries.Select(e => e.MemberId).Distinct().ToList();
        var members = await _context.Members.Where(m => memberIds.Contains(m.Id)).ToListAsync();
        if (members.Count != memberIds.Count)
        {
            return BadRequest(new { error = "One or more members not found" });
        }

        var friday = new Friday
        {
            AccountId = dto.AccountId,
            Account = account,
            BetDateTime = dto.BetDateTime ?? DateTime.UtcNow.AddHours(7),
            TotalOddsHongKong = dto.TotalOddsHongKong,
            TotalOddsInternational = dto.TotalOddsInternational,
            TotalDeposit = dto.TotalDeposit,
            Status = dto.Status,
            Dog = dto.Dog?.Trim()
        };

        // Add lineup entries
        foreach (var entryDto in dto.LineupEntries)
        {
            var member = members.First(m => m.Id == entryDto.MemberId);
            friday.LineupEntries.Add(new LineupEntry
            {
                MemberId = entryDto.MemberId,
                Member = member,
                Amount = entryDto.Amount,
                Friday = friday
            });
        }

        _context.Fridays.Add(friday);
        await _context.SaveChangesAsync();

        // Add SingleBets if provided
        if (dto.SingleBets != null && dto.SingleBets.Count > 0)
        {
            if (dto.SingleBets.Count != 3)
            {
                return BadRequest(new { error = "Friday must have exactly 3 SingleBets" });
            }

            foreach (var sbDto in dto.SingleBets)
            {
                var singleBet = new SingleBet
                {
                    Title = sbDto.Title,
                    MatchStartTime = sbDto.MatchStartTime,
                    MatchEndTime = sbDto.MatchEndTime,
                    OddsHongKong = sbDto.OddsHongKong,
                    OddsInternational = sbDto.OddsInternational,
                    Status = sbDto.Status,
                    FridayId = friday.Id,
                    Friday = friday
                };
                _context.SingleBets.Add(singleBet);
            }
            await _context.SaveChangesAsync();
        }

        // Create HedgeSet if provided or if CreateHedgeSet is true (legacy)
        if (dto.HedgeSet != null)
        {
            if (dto.HedgeSet.SingleBets == null || dto.HedgeSet.SingleBets.Count != 2)
            {
                return BadRequest(new { error = "HedgeSet must have exactly 2 SingleBets" });
            }

            var hedgeSet = new HedgeSet
            {
                FridayId = friday.Id,
                Friday = friday,
                Title = dto.HedgeSet.Title,
                Budget = dto.HedgeSet.Budget
            };
            _context.HedgeSets.Add(hedgeSet);
            await _context.SaveChangesAsync();

            // Add SingleBets to HedgeSet
            foreach (var sbDto in dto.HedgeSet.SingleBets)
            {
                var singleBet = new SingleBet
                {
                    Title = sbDto.Title,
                    MatchStartTime = sbDto.MatchStartTime,
                    MatchEndTime = sbDto.MatchEndTime,
                    OddsHongKong = sbDto.OddsHongKong,
                    OddsInternational = sbDto.OddsInternational,
                    Status = sbDto.Status,
                    HedgeSetId = hedgeSet.Id,
                    HedgeSet = hedgeSet
                };
                _context.SingleBets.Add(singleBet);
            }
            await _context.SaveChangesAsync();
        }
        else if (dto.CreateHedgeSet) // Legacy support
        {
            var hedgeSet = new HedgeSet
            {
                FridayId = friday.Id,
                Friday = friday,
                Title = $"Withdrawal bets for {friday.BetDateTime:yyyy-MM-dd}",
                Budget = friday.TotalDeposit * 2
            };
            _context.HedgeSets.Add(hedgeSet);
            await _context.SaveChangesAsync();
        }

        // Reload Friday with all related data to return as DTO
        var createdFriday = await _context.Fridays
            .Include(f => f.Account)
            .Include(f => f.LineupEntries)
                .ThenInclude(le => le.Member)
            .Include(f => f.SingleBets)
            .Include(f => f.HedgeSet)
            .FirstOrDefaultAsync(f => f.Id == friday.Id);

        if (createdFriday == null)
        {
            return NotFound();
        }

        var today = DateTime.UtcNow.AddHours(7).Date;
        var fridayDto = new FridayDto
        {
            Id = createdFriday.Id,
            AccountId = createdFriday.AccountId,
            AccountTitle = createdFriday.Account.Title,
            BetDateTime = createdFriday.BetDateTime,
            TotalOddsHongKong = createdFriday.TotalOddsHongKong,
            TotalOddsInternational = createdFriday.TotalOddsInternational,
            TotalDeposit = createdFriday.TotalDeposit,
            Status = createdFriday.Status,
            Dog = createdFriday.Dog,
            IsCurrentFriday = createdFriday.BetDateTime.Date == today,
            HasHedgeSet = createdFriday.HedgeSet != null,
            LineupEntries = createdFriday.LineupEntries.Select(le => new LineupEntryDto
            {
                Id = le.Id,
                MemberId = le.MemberId,
                MemberName = le.Member.Name,
                Amount = le.Amount
            }).ToList(),
            SingleBets = createdFriday.SingleBets.Where(sb => sb.FridayId == createdFriday.Id).Select(sb => new SingleBetDto
            {
                Id = sb.Id,
                Title = sb.Title,
                MatchStartTime = sb.MatchStartTime,
                MatchEndTime = sb.MatchEndTime,
                OddsHongKong = sb.OddsHongKong,
                OddsInternational = sb.OddsInternational,
                Status = sb.Status
            }).ToList()
        };

        return CreatedAtAction(nameof(GetFriday), new { id = fridayDto.Id }, fridayDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFriday(int id, [FromBody] UpdateFridayDto dto)
    {
        var friday = await _context.Fridays
            .Include(f => f.Account)
            .Include(f => f.LineupEntries)
            .Include(f => f.SingleBets)
            .Include(f => f.HedgeSet)
                .ThenInclude(hs => hs != null ? hs.SingleBets : null!)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (friday == null)
        {
            return NotFound();
        }

        // Log received BetDateTime for debugging
        _logger.LogInformation("UpdateFriday: Received BetDateTime = {BetDateTime}, HasValue = {HasValue}", 
            dto.BetDateTime, dto.BetDateTime.HasValue);

        if (dto.AccountId.HasValue && dto.AccountId.Value != friday.AccountId)
        {
            var account = await _context.Accounts.FindAsync(dto.AccountId.Value);
            if (account == null)
            {
                return BadRequest(new { error = "Account not found" });
            }
            friday.AccountId = dto.AccountId.Value;
            friday.Account = account;
        }
        
        // Always update BetDateTime if provided
        // For nullable DateTime, HasValue will be true if a value was sent
        if (dto.BetDateTime.HasValue)
        {
            _logger.LogInformation("UpdateFriday: Updating BetDateTime from {Old} to {New}", 
                friday.BetDateTime, dto.BetDateTime.Value);
            friday.BetDateTime = dto.BetDateTime.Value;
        }
        else
        {
            _logger.LogWarning("UpdateFriday: BetDateTime not provided or HasValue is false. Current value: {Current}", 
                friday.BetDateTime);
        }

        if (dto.TotalOddsHongKong.HasValue)
        {
            friday.TotalOddsHongKong = dto.TotalOddsHongKong.Value;
        }

        if (dto.TotalOddsInternational.HasValue)
        {
            friday.TotalOddsInternational = dto.TotalOddsInternational.Value;
        }

        if (dto.TotalDeposit.HasValue)
        {
            if (dto.TotalDeposit.Value < 0.01m || dto.TotalDeposit.Value >= 8100000m)
            {
                return BadRequest(new { error = "TotalDeposit must be between 0.01 and 8,099,999.99" });
            }
            friday.TotalDeposit = dto.TotalDeposit.Value;
        }

        if (dto.Status.HasValue)
        {
            friday.Status = dto.Status.Value;
        }

        if (dto.Dog != null)
        {
            friday.Dog = dto.Dog.Trim().Length > 0 ? dto.Dog.Trim() : null;
        }

        // Update LineupEntries if provided
        if (dto.LineupEntries != null && dto.LineupEntries.Count > 0)
        {
            // Validate lineup total
            var lineupTotal = dto.LineupEntries.Sum(le => le.Amount);
            if (Math.Abs(lineupTotal - friday.TotalDeposit) >= 0.01m)
            {
                return BadRequest(new { error = $"Lineup total ({lineupTotal}) must equal Total Deposit ({friday.TotalDeposit})" });
            }

            // Remove existing lineup entries
            _context.LineupEntries.RemoveRange(friday.LineupEntries);

            // Add new lineup entries
            foreach (var leDto in dto.LineupEntries)
            {
                var member = await _context.Members.FindAsync(leDto.MemberId);
                if (member == null)
                {
                    return BadRequest(new { error = $"Member with ID {leDto.MemberId} not found" });
                }

                var lineupEntry = new LineupEntry
                {
                    FridayId = friday.Id,
                    Friday = friday,
                    MemberId = leDto.MemberId,
                    Member = member,
                    Amount = leDto.Amount
                };
                _context.LineupEntries.Add(lineupEntry);
            }
        }

        // Update SingleBets if provided
        if (dto.SingleBets != null)
        {
            if (dto.SingleBets.Count != 3)
            {
                return BadRequest(new { error = "Friday must have exactly 3 SingleBets" });
            }

            // Remove existing single bets (only those belonging to Friday, not HedgeSet)
            var existingFridaySingleBets = friday.SingleBets.Where(sb => sb.FridayId == friday.Id && sb.HedgeSetId == null).ToList();
            _context.SingleBets.RemoveRange(existingFridaySingleBets);

            // Add new single bets
            foreach (var sbDto in dto.SingleBets)
            {
                var singleBet = new SingleBet
                {
                    FridayId = friday.Id,
                    Friday = friday,
                    Title = sbDto.Title,
                    MatchStartTime = sbDto.MatchStartTime,
                    MatchEndTime = sbDto.MatchEndTime,
                    OddsHongKong = sbDto.OddsHongKong,
                    OddsInternational = sbDto.OddsInternational,
                    Status = sbDto.Status
                };
                _context.SingleBets.Add(singleBet);
            }
        }

        // Update HedgeSet if provided
        if (dto.HedgeSet != null)
        {
            if (dto.HedgeSet.SingleBets == null || dto.HedgeSet.SingleBets.Count != 2)
            {
                return BadRequest(new { error = "HedgeSet must have exactly 2 SingleBets" });
            }

            // Delete existing hedge set and its single bets
            if (friday.HedgeSet != null)
            {
                var existingHedgeSetSingleBets = await _context.SingleBets
                    .Where(sb => sb.HedgeSetId == friday.HedgeSet.Id)
                    .ToListAsync();
                _context.SingleBets.RemoveRange(existingHedgeSetSingleBets);
                _context.HedgeSets.Remove(friday.HedgeSet);
                await _context.SaveChangesAsync();
            }

            // Create new hedge set
            var hedgeSet = new HedgeSet
            {
                FridayId = friday.Id,
                Friday = friday,
                Title = dto.HedgeSet.Title,
                Budget = dto.HedgeSet.Budget
            };
            _context.HedgeSets.Add(hedgeSet);
            await _context.SaveChangesAsync();

            // Add SingleBets to HedgeSet
            foreach (var sbDto in dto.HedgeSet.SingleBets)
            {
                var singleBet = new SingleBet
                {
                    HedgeSetId = hedgeSet.Id,
                    HedgeSet = hedgeSet,
                    Title = sbDto.Title,
                    MatchStartTime = sbDto.MatchStartTime,
                    MatchEndTime = sbDto.MatchEndTime,
                    OddsHongKong = sbDto.OddsHongKong,
                    OddsInternational = sbDto.OddsInternational,
                    Status = sbDto.Status
                };
                _context.SingleBets.Add(singleBet);
            }
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFriday(int id)
    {
        var friday = await _context.Fridays
            .Include(f => f.HedgeSet)
            .FirstOrDefaultAsync(f => f.Id == id);
        
        if (friday == null)
        {
            return NotFound();
        }

        // Delete SingleBets belonging to HedgeSet first (NO ACTION constraint)
        if (friday.HedgeSet != null)
        {
            var hedgeSetSingleBets = await _context.SingleBets
                .Where(sb => sb.HedgeSetId == friday.HedgeSet.Id)
                .ToListAsync();
            
            if (hedgeSetSingleBets.Any())
            {
                _context.SingleBets.RemoveRange(hedgeSetSingleBets);
                await _context.SaveChangesAsync();
            }
        }

        // Delete Friday (will cascade delete: SingleBets via FridayId, LineupEntries, and HedgeSet)
        _context.Fridays.Remove(friday);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("accounts")]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts()
    {
        return await _context.Accounts
            .Select(a => new AccountDto
            {
                Id = a.Id,
                Title = a.Title
            })
            .ToListAsync();
    }
}

public class UpdateFridayDto
{
    public int? AccountId { get; set; }
    public DateTime? BetDateTime { get; set; }
    public decimal? TotalOddsHongKong { get; set; }
    public decimal? TotalOddsInternational { get; set; }
    public decimal? TotalDeposit { get; set; }
    public BetStatus? Status { get; set; }
    public string? Dog { get; set; }
    public List<LineupEntryCreateDto>? LineupEntries { get; set; }
    public List<SingleBetCreateDto>? SingleBets { get; set; }
    public HedgeSetCreateDto? HedgeSet { get; set; }
}

public class FridayDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountTitle { get; set; } = string.Empty;
    public DateTime BetDateTime { get; set; }
    public decimal TotalOddsHongKong { get; set; }
    public decimal TotalOddsInternational { get; set; }
    public decimal TotalDeposit { get; set; }
    public BetStatus Status { get; set; }
    public string? Dog { get; set; }
    public bool IsCurrentFriday { get; set; }
    public bool HasHedgeSet { get; set; }
    public List<LineupEntryDto> LineupEntries { get; set; } = new();
    public List<SingleBetDto> SingleBets { get; set; } = new();
    public FridayHedgeSetDto? HedgeSet { get; set; }
}

public class FridayHedgeSetDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public List<SingleBetDto> SingleBets { get; set; } = new();
}

public class LineupEntryDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class SingleBetDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime MatchStartTime { get; set; }
    public DateTime MatchEndTime { get; set; }
    public decimal OddsHongKong { get; set; }
    public decimal OddsInternational { get; set; }
    public BetStatus Status { get; set; }
}

public class CreateFridayDto
{
    public int AccountId { get; set; }
    public DateTime? BetDateTime { get; set; }
    public decimal TotalOddsHongKong { get; set; }
    public decimal TotalOddsInternational { get; set; }
    public decimal TotalDeposit { get; set; }
    public BetStatus Status { get; set; } = BetStatus.Running;
    public string? Dog { get; set; }
    public List<LineupEntryCreateDto>? LineupEntries { get; set; }
    public List<SingleBetCreateDto>? SingleBets { get; set; }
    public HedgeSetCreateDto? HedgeSet { get; set; }
    public bool CreateHedgeSet { get; set; } = false; // Legacy support
}

public class SingleBetCreateDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime MatchStartTime { get; set; }
    public DateTime MatchEndTime { get; set; }
    public decimal OddsHongKong { get; set; }
    public decimal OddsInternational { get; set; }
    public BetStatus Status { get; set; }
}

public class HedgeSetCreateDto
{
    public string Title { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public List<SingleBetCreateDto> SingleBets { get; set; } = new();
}

public class LineupEntryCreateDto
{
    public int MemberId { get; set; }
    public decimal Amount { get; set; }
}

public class AccountDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

