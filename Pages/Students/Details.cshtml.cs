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
    public List<AttendanceLog> Attendances { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var student = await _context.Students
            .Include(s => s.Plan)
            .Include(s => s.Payments)
            .Include(s => s.AttendanceLogs)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null) return NotFound();

        Student = student;
        Payments = student.Payments.OrderByDescending(p => p.DueDate).ToList();
        Attendances = student.AttendanceLogs.OrderByDescending(a => a.EntryTime).Take(20).ToList();

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
}
