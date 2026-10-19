using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BoxeGymManager.Data;
using BoxeGymManager.Models;

namespace BoxeGymManager.Pages.Plans;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Plan Plan { get; set; } = new();

    public void OnGet()
    {
        Plan.Price = 199.90m;
        Plan.DaysPerWeek = 3;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Plan.Name) || Plan.Price <= 0)
        {
            ModelState.AddModelError("", "Nome e preço válido são obrigatórios.");
            return Page();
        }

        Plan.Name = Plan.Name.Trim();
        Plan.Description = Plan.Description?.Trim() ?? string.Empty;

        _context.Plans.Add(Plan);
        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}
