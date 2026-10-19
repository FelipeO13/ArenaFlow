using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BoxeGymManager.Data;
using BoxeGymManager.Models;

namespace BoxeGymManager.Pages.Schedule;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public BoxingClass BoxingClass { get; set; } = new();

    public void OnGet()
    {
        BoxingClass.StartTime = new TimeSpan(19, 0, 0);
        BoxingClass.EndTime = new TimeSpan(20, 0, 0);
        BoxingClass.MaxCapacity = 15;
        BoxingClass.RingOrRoom = "Ringue 1";
        BoxingClass.LevelRequirement = "Todos os níveis";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(BoxingClass.Title) || string.IsNullOrWhiteSpace(BoxingClass.CoachName))
        {
            ModelState.AddModelError("", "Título e nome do treinador são obrigatórios.");
            return Page();
        }

        BoxingClass.Title = BoxingClass.Title.Trim();
        BoxingClass.CoachName = BoxingClass.CoachName.Trim();
        BoxingClass.LevelRequirement = string.IsNullOrWhiteSpace(BoxingClass.LevelRequirement) ? "Todos os níveis" : BoxingClass.LevelRequirement.Trim();
        BoxingClass.RingOrRoom = string.IsNullOrWhiteSpace(BoxingClass.RingOrRoom) ? "Ringue Principal" : BoxingClass.RingOrRoom.Trim();

        _context.BoxingClasses.Add(BoxingClass);
        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}
