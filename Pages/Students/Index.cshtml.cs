using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using BoxeGymManager.Models;

namespace BoxeGymManager.Pages.Students;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Student> Students { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public BoxingLevel? LevelFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public StudentStatus? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? ActiveOnly { get; set; }

    public async Task OnGetAsync()
    {
        var query = _context.Students
            .Include(s => s.Plan)
            .Include(s => s.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim().ToLower();
            query = query.Where(x => x.FullName.ToLower().Contains(s) 
                                  || x.RegistrationCode.ToLower().Contains(s) 
                                  || x.CPF.Contains(s));
        }

        if (LevelFilter.HasValue)
        {
            query = query.Where(x => x.ExperienceLevel == LevelFilter.Value);
        }

        if (StatusFilter.HasValue)
        {
            query = query.Where(x => x.Status == StatusFilter.Value);
        }
        else if (ActiveOnly.HasValue)
        {
            query = query.Where(x => x.IsActive == ActiveOnly.Value);
        }

        Students = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
    }
}
