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
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(BoxingClass.CoachName))
        {
            ModelState.AddModelError("BoxingClass.CoachName", "O nome do instrutor é obrigatório.");
            return Page();
        }

        if (BoxingClass.EndTime <= BoxingClass.StartTime)
        {
            ModelState.AddModelError("BoxingClass.EndTime", "O horário de término deve ser posterior ao horário de início.");
            return Page();
        }

        if (BoxingClass.MaxCapacity <= 0)
        {
            ModelState.AddModelError("BoxingClass.MaxCapacity", "O limite de alunos deve ser maior que zero.");
            return Page();
        }

        BoxingClass.CoachName = BoxingClass.CoachName.Trim();
        BoxingClass.Description = string.IsNullOrWhiteSpace(BoxingClass.Description) ? null : BoxingClass.Description.Trim();
        BoxingClass.Title = BoxingClass.DisplayName;
        BoxingClass.LevelRequirement = "Todos os níveis";
        BoxingClass.RingOrRoom = "Academia";

        _context.BoxingClasses.Add(BoxingClass);
        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}
