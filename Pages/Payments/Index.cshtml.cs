using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using BoxeGymManager.Models;

namespace BoxeGymManager.Pages.Payments;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Payment> Payments { get; set; } = new();

    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalOverdue { get; set; }

    [BindProperty(SupportsGet = true)]
    public PaymentStatus? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        var query = _context.Payments
            .Include(p => p.Student)
            .AsQueryable();

        if (StatusFilter.HasValue)
        {
            query = query.Where(p => p.Status == StatusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim().ToLower();
            query = query.Where(p => p.Student!.FullName.ToLower().Contains(s) 
                                  || p.Student.RegistrationCode.ToLower().Contains(s)
                                  || p.ReceiptCode.ToLower().Contains(s));
        }

        Payments = await query.OrderByDescending(p => p.DueDate).ToListAsync();

        var all = await _context.Payments.ToListAsync();
        TotalPaid = all.Where(p => p.Status == PaymentStatus.Pago).Sum(p => p.Amount);
        TotalPending = all.Where(p => p.Status == PaymentStatus.Pendente && p.DueDate >= DateTime.Today).Sum(p => p.Amount);
        TotalOverdue = all.Where(p => p.Status == PaymentStatus.Atrasado || (p.Status == PaymentStatus.Pendente && p.DueDate < DateTime.Today)).Sum(p => p.Amount);
    }

    public async Task<IActionResult> OnPostMarkAsPaidAsync(int id, PaymentMethod method)
    {
        var payment = await _context.Payments
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment != null)
        {
            payment.Status = PaymentStatus.Pago;
            payment.PaidDate = DateTime.Now;
            payment.Method = method;

            // Se o plano do aluno estava vencido, renovar de acordo com o plano
            if (payment.Student?.PlanId != null)
            {
                var plan = await _context.Plans.FindAsync(payment.Student.PlanId);
                if (plan != null)
                {
                    var baseDate = payment.Student.PlanEndDate.HasValue && payment.Student.PlanEndDate.Value > DateTime.Today 
                        ? payment.Student.PlanEndDate.Value 
                        : DateTime.Today;

                    payment.Student.PlanEndDate = plan.BillingCycle switch
                    {
                        PlanBillingCycle.Trimestral => baseDate.AddMonths(3),
                        PlanBillingCycle.Semestral => baseDate.AddMonths(6),
                        PlanBillingCycle.Anual => baseDate.AddYears(1),
                        _ => baseDate.AddMonths(1)
                    };
                }
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}
