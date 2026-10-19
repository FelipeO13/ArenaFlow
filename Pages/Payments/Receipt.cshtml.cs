using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using BoxeGymManager.Models;

namespace BoxeGymManager.Pages.Payments;

public class ReceiptModel : PageModel
{
    private readonly AppDbContext _context;

    public ReceiptModel(AppDbContext context)
    {
        _context = context;
    }

    public Payment Payment { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var payment = await _context.Payments
            .Include(p => p.Student)
                .ThenInclude(s => s!.Plan)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null) return NotFound();

        Payment = payment;
        return Page();
    }
}
