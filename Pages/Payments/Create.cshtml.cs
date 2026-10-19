using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using BoxeGymManager.Models;

namespace BoxeGymManager.Pages.Payments;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Payment Payment { get; set; } = new();

    public SelectList StudentsList { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? studentId)
    {
        Payment.DueDate = DateTime.Today.AddDays(5);
        Payment.ReferenceMonthYear = $"{DateTime.Today.Month:D2}/{DateTime.Today.Year}";
        Payment.ReceiptCode = $"REC-{DateTime.Today:yyyyMM}-{Random.Shared.Next(100, 999)}";
        Payment.Status = PaymentStatus.Pendente;

        if (studentId.HasValue)
        {
            var student = await _context.Students.Include(s => s.Plan).FirstOrDefaultAsync(s => s.Id == studentId.Value);
            if (student != null)
            {
                Payment.StudentId = student.Id;
                if (student.Plan != null)
                {
                    Payment.Amount = student.Plan.Price;
                    Payment.Notes = $"Mensalidade - {student.Plan.Name}";
                }
            }
        }

        await LoadStudentsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Payment.StudentId == 0 || Payment.Amount <= 0)
        {
            ModelState.AddModelError("", "Aluno e valor válido são obrigatórios.");
            await LoadStudentsAsync();
            return Page();
        }

        if (Payment.Status == PaymentStatus.Pago && !Payment.PaidDate.HasValue)
        {
            Payment.PaidDate = DateTime.Now;
        }

        Payment.ReferenceMonthYear = Payment.ReferenceMonthYear?.Trim() ?? $"{DateTime.Today.Month:D2}/{DateTime.Today.Year}";
        Payment.ReceiptCode = Payment.ReceiptCode?.Trim() ?? $"REC-{DateTime.Today:yyyyMM}-{Random.Shared.Next(100, 999)}";
        Payment.Notes = Payment.Notes?.Trim() ?? string.Empty;

        Payment.CreatedAt = DateTime.Now;
        _context.Payments.Add(Payment);

        // Se pago e o aluno tem plano, atualizar data de término do plano
        if (Payment.Status == PaymentStatus.Pago)
        {
            var student = await _context.Students.Include(s => s.Plan).FirstOrDefaultAsync(s => s.Id == Payment.StudentId);
            if (student?.Plan != null)
            {
                var baseDate = student.PlanEndDate.HasValue && student.PlanEndDate.Value > DateTime.Today 
                    ? student.PlanEndDate.Value 
                    : DateTime.Today;

                student.PlanEndDate = student.Plan.BillingCycle switch
                {
                    PlanBillingCycle.Trimestral => baseDate.AddMonths(3),
                    PlanBillingCycle.Semestral => baseDate.AddMonths(6),
                    PlanBillingCycle.Anual => baseDate.AddYears(1),
                    _ => baseDate.AddMonths(1)
                };
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToPage("./Receipt", new { id = Payment.Id });
    }

    private async Task LoadStudentsAsync()
    {
        var students = await _context.Students
            .Where(s => s.IsActive)
            .OrderBy(s => s.FullName)
            .Select(s => new { s.Id, Display = $"{s.FullName} ({s.RegistrationCode})" })
            .ToListAsync();

        StudentsList = new SelectList(students, "Id", "Display");
    }
}
