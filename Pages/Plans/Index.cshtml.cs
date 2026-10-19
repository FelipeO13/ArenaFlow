using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using BoxeGymManager.Models;

namespace BoxeGymManager.Pages.Plans;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public List<PlanViewModel> Plans { get; set; } = new();

    public class PlanViewModel
    {
        public Plan Plan { get; set; } = null!;
        public int StudentCount { get; set; }
    }

    public async Task OnGetAsync()
    {
        var plans = await _context.Plans.ToListAsync();
        var counts = await _context.Students
            .Where(s => s.PlanId != null && s.IsActive)
            .GroupBy(s => s.PlanId)
            .Select(g => new { PlanId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PlanId!.Value, x => x.Count);

        Plans = plans.Select(p => new PlanViewModel
        {
            Plan = p,
            StudentCount = counts.TryGetValue(p.Id, out var count) ? count : 0
        }).ToList();
    }
}
