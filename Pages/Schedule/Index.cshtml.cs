using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using BoxeGymManager.Models;

namespace BoxeGymManager.Pages.Schedule;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public Dictionary<DayOfWeek, List<BoxingClass>> ClassesByDay { get; set; } = new();

    public async Task OnGetAsync()
    {
        var classes = await _context.BoxingClasses
            .Include(c => c.Students)
            .Where(c => c.IsActive)
            .ToListAsync();
        classes = classes.OrderBy(c => c.StartTime).ToList();

        var days = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday
        };

        ClassesByDay = days.ToDictionary(
            day => day,
            day => classes.Where(c => c.DayOfWeek == day).ToList()
        );
    }
}
