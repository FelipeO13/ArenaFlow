using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BoxeGymManager.Data;
using BoxeGymManager.Models;

namespace BoxeGymManager.Pages.Plans;

public class EditModel : PageModel
{
    private readonly AppDbContext _context;

    public EditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Plan Plan { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var plan = await _context.Plans.FindAsync(id);
        if (plan == null) return NotFound();

        Plan = plan;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Plan.Name) || Plan.Price <= 0)
        {
            ModelState.AddModelError("", "Nome e valor válido são obrigatórios.");
            return Page();
        }

        var dbPlan = await _context.Plans.FindAsync(Plan.Id);
        if (dbPlan == null) return NotFound();

        dbPlan.Name = Plan.Name.Trim();
        dbPlan.Description = Plan.Description?.Trim() ?? string.Empty;
        dbPlan.Price = Plan.Price;
        dbPlan.BillingCycle = Plan.BillingCycle;
        dbPlan.DaysPerWeek = Plan.DaysPerWeek;
        dbPlan.IncludesSparring = Plan.IncludesSparring;
        dbPlan.IsActive = Plan.IsActive;

        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}
