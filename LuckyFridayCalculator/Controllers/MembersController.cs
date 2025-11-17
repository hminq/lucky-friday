using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuckyFridayCalculator.Models;

namespace LuckyFridayCalculator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly LuckyFridayDbContext _context;

    public MembersController(LuckyFridayDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
    {
        return await _context.Members
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMember(int id)
    {
        var member = await _context.Members.FindAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        return member;
    }

    [HttpPost]
    public async Task<ActionResult<Member>> CreateMember([FromBody] CreateMemberDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { error = "Name is required" });
        }

        if (dto.Name.Length > 100)
        {
            return BadRequest(new { error = "Name must not exceed 100 characters" });
        }

        var member = new Member
        {
            Name = dto.Name.Trim()
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMember), new { id = member.Id }, member);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(int id, [FromBody] UpdateMemberDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { error = "Name is required" });
        }

        if (dto.Name.Length > 100)
        {
            return BadRequest(new { error = "Name must not exceed 100 characters" });
        }

        var member = await _context.Members.FindAsync(id);
        if (member == null)
        {
            return NotFound();
        }

        member.Name = dto.Name.Trim();
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var member = await _context.Members
            .Include(m => m.LineupEntries)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member == null)
        {
            return NotFound();
        }

        if (member.LineupEntries.Any())
        {
            return BadRequest(new { error = "Cannot delete member with existing lineup entries" });
        }

        _context.Members.Remove(member);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateMemberDto
{
    public required string Name { get; set; }
}

public class UpdateMemberDto
{
    public required string Name { get; set; }
}

