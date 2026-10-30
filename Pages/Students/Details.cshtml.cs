using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using BoxeGymManager.Models;

namespace BoxeGymManager.Pages.Students;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _context;

    public DetailsModel(AppDbContext context)
    {
        _context = context;
    }

    public Student Student { get; set; } = null!;
    public List<Payment> Payments { get; set; } = new();
    public Payment? LastPayment { get; set; }
    public string PaymentStatusText { get; set; } = "Em dia";
    public string PaymentStatusBadgeClass { get; set; } = "bg-success";
    public int MonthlyCheckInsCount { get; set; }
    public List<AttendanceLog> RecentAttendances { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var student = await _context.Students
            .Include(s => s.Plan)
            .Include(s => s.BoxingClass)
                .ThenInclude(c => c!.Students)
            .Include(s => s.Payments)
            .Include(s => s.AttendanceLogs)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null) return NotFound();

        Student = student;
        Payments = student.Payments.OrderByDescending(p => p.DueDate).ToList();
        
        LastPayment = student.Payments
            .Where(p => p.Status == PaymentStatus.Pago)
            .OrderByDescending(p => p.PaidDate ?? p.DueDate)
            .FirstOrDefault();

        var today = DateTime.Today;
        var hasOverdue = student.Payments.Any(p => p.Status == PaymentStatus.Atrasado || (p.Status == PaymentStatus.Pendente && p.DueDate < today))
                         || (student.PlanEndDate.HasValue && student.PlanEndDate.Value < today);

        var hasPendingNearDue = student.Payments.Any(p => p.Status == PaymentStatus.Pendente && p.DueDate >= today)
                                || (student.PlanEndDate.HasValue && student.PlanEndDate.Value >= today && student.PlanEndDate.Value <= today.AddDays(7));

        if (hasOverdue)
        {
            PaymentStatusText = "Atrasado";
            PaymentStatusBadgeClass = "bg-danger";
        }
        else if (hasPendingNearDue)
        {
            PaymentStatusText = "A Vencer";
            PaymentStatusBadgeClass = "bg-warning text-dark";
        }
        else if (student.Payments.Any(p => p.Status == PaymentStatus.Pago) || (student.PlanEndDate.HasValue && student.PlanEndDate.Value >= today))
        {
            PaymentStatusText = "Em dia";
            PaymentStatusBadgeClass = "bg-success";
        }
        else
        {
            PaymentStatusText = "Sem pagamentos";
            PaymentStatusBadgeClass = "bg-secondary";
        }

        MonthlyCheckInsCount = student.AttendanceLogs
            .Count(a => a.EntryTime.Year == today.Year && a.EntryTime.Month == today.Month
                     && a.AccessStatus != AccessStatus.BloqueadoInativo && a.AccessStatus != AccessStatus.BloqueadoInadimplente);

        RecentAttendances = student.AttendanceLogs
            .OrderByDescending(a => a.EntryTime)
            .Take(10)
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostChangeStatusAsync(int id, StudentStatus newStatus, string? reason)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return NotFound();

        student.Status = newStatus;
        student.StatusReason = reason?.Trim() ?? string.Empty;
        student.StatusChangedAt = DateTime.Now;
        student.IsActive = newStatus == StudentStatus.Ativo;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = newStatus switch
        {
            StudentStatus.Ativo => $"Matrícula de {student.FullName} REATIVADA com sucesso! Acesso aos treinos liberado.",
            StudentStatus.Trancado => $"Matrícula de {student.FullName} TRANCADA temporariamente. Acesso pausado.",
            StudentStatus.Cancelado => $"Inscrição de {student.FullName} CANCELADA com sucesso.",
            _ => "Status da matrícula atualizado."
        };

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var student = await _context.Students
            .Include(s => s.Payments)
            .Include(s => s.AttendanceLogs)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            TempData["ErrorMessage"] = "Aluno não localizado para exclusão.";
            return RedirectToPage("./Index");
        }

        var studentName = student.FullName;
        var regCode = student.RegistrationCode;

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            if (student.Payments.Any())
            {
                _context.Payments.RemoveRange(student.Payments);
            }

            if (student.AttendanceLogs.Any())
            {
                _context.AttendanceLogs.RemoveRange(student.AttendanceLogs);
            }

            _context.Students.Remove(student);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["SuccessMessage"] = $"Aluno {studentName} ({regCode}) foi excluído definitivamente do sistema.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Erro ao tentar excluir o aluno: {ex.Message}";
            return RedirectToPage(new { id });
        }
    }
}
