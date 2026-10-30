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
                    var hasPaid = await _context.Payments.AnyAsync(p => p.StudentId == student.Id && p.Status == PaymentStatus.Pago);
                    if (!hasPaid && student.DiaVencimento.HasValue)
                    {
                        var start = student.PlanStartDate ?? student.CreatedAt;
                        var res = BoxeGymManager.Services.CalculadoraVencimento.CalcularPrimeiroVencimento(student.Plan.Price, student.DiaVencimento.Value, start);
                        Payment.Amount = res.ValorPrimeiroPagamento;
                        Payment.DueDate = res.PrimeiroVencimento;
                        Payment.ReferenceMonthYear = $"{res.PrimeiroVencimento.Month:D2}/{res.PrimeiroVencimento.Year}";
                        var sinal = res.DiferencaDias > 0 ? $"+{res.DiferencaDias}" : $"{res.DiferencaDias}";
                        var tipo = res.DiferencaDias < 0 ? $"desconto de {Math.Abs(res.ValorAjuste):C}" : (res.DiferencaDias > 0 ? $"acréscimo de {res.ValorAjuste:C}" : "sem ajuste");
                        Payment.Notes = $"1ª Mensalidade proporcional ({sinal} dias, {tipo}) - Plano {student.Plan.Name} (Venc. dia {student.DiaVencimento})";
                    }
                    else
                    {
                        Payment.Amount = student.Plan.Price;
                        Payment.Notes = $"Mensalidade - {student.Plan.Name}";
                        if (student.DiaVencimento.HasValue && student.PlanEndDate.HasValue)
                        {
                            var proximaValidade = BoxeGymManager.Services.CalculadoraVencimento.CalcularProximaValidade(student, student.Plan);
                            Payment.DueDate = proximaValidade;
                            Payment.ReferenceMonthYear = $"{proximaValidade.Month:D2}/{proximaValidade.Year}";
                        }
                    }
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
                student.PlanEndDate = BoxeGymManager.Services.CalculadoraVencimento.CalcularProximaValidade(student, student.Plan);
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
