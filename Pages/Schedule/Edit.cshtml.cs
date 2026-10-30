using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using BoxeGymManager.Models;

namespace BoxeGymManager.Pages.Schedule;

public class EditModel : PageModel
{
    private readonly AppDbContext _context;

    public EditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public BoxingClass BoxingClass { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var boxingClass = await _context.BoxingClasses.FirstOrDefaultAsync(m => m.Id == id);
        if (boxingClass == null)
        {
            return NotFound();
        }

        BoxingClass = boxingClass;
        return Page();
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

        var classToUpdate = await _context.BoxingClasses.FirstOrDefaultAsync(c => c.Id == BoxingClass.Id);
        if (classToUpdate == null)
        {
            return NotFound();
        }

        classToUpdate.Modality = BoxingClass.Modality;
        classToUpdate.DayOfWeek = BoxingClass.DayOfWeek;
        classToUpdate.StartTime = BoxingClass.StartTime;
        classToUpdate.EndTime = BoxingClass.EndTime;
        classToUpdate.CoachName = BoxingClass.CoachName.Trim();
        classToUpdate.MaxCapacity = BoxingClass.MaxCapacity;
        classToUpdate.Description = string.IsNullOrWhiteSpace(BoxingClass.Description) ? null : BoxingClass.Description.Trim();
        classToUpdate.Title = classToUpdate.DisplayName;

        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}
