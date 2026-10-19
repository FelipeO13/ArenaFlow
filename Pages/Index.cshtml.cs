using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using BoxeGymManager.Models;

namespace BoxeGymManager.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public int TotalActiveStudents { get; set; }
    public int CheckInsToday { get; set; }
    public int CurrentlyInGym { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int OverduePaymentsCount { get; set; }

    public List<BoxingClass> TodayClasses { get; set; } = new();
    public List<AttendanceLog> RecentAttendances { get; set; } = new();
    public List<Payment> PendingPayments { get; set; } = new();

    [BindProperty]
    public string QuickCode { get; set; } = string.Empty;

    public string? QuickCheckInFeedback { get; set; }
    public string? QuickCheckInStatusClass { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDashboardDataAsync();
    }

    public async Task<IActionResult> OnPostQuickCheckInAsync()
    {
        if (string.IsNullOrWhiteSpace(QuickCode))
        {
            QuickCheckInFeedback = "Por favor, informe a matrícula ou CPF do atleta.";
            QuickCheckInStatusClass = "danger";
            await LoadDashboardDataAsync();
            return Page();
        }

        var query = QuickCode.Trim();
        var student = await _context.Students
            .Include(s => s.Plan)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.RegistrationCode.ToLower() == query.ToLower() || s.CPF.Replace(".", "").Replace("-", "") == query.Replace(".", "").Replace("-", ""));

        if (student == null)
        {
            QuickCheckInFeedback = $"Aluno com código/CPF '{QuickCode}' não foi localizado no sistema.";
            QuickCheckInStatusClass = "danger";
            await LoadDashboardDataAsync();
            return Page();
        }

        // Verificar se já está dentro da academia (saída rápida)
        var today = DateTime.Today;
        var openSession = await _context.AttendanceLogs
            .Where(a => a.StudentId == student.Id && a.EntryTime >= today && a.ExitTime == null)
            .OrderByDescending(a => a.EntryTime)
            .FirstOrDefaultAsync();

        if (openSession != null)
        {
            // Registrar Saída
            openSession.ExitTime = DateTime.Now;
            await _context.SaveChangesAsync();
            QuickCheckInFeedback = $"Saída registrada para {student.FullName}! Bom descanso campeão(ã)!";
            QuickCheckInStatusClass = "info";
            await LoadDashboardDataAsync();
            return Page();
        }

        // Registrar Entrada e checar status
        var status = AccessStatus.Autorizado;
        var message = "Entrada autorizada! Bons treinos!";

        if (!student.IsActive)
        {
            status = AccessStatus.BloqueadoInativo;
            message = "Acesso negado: Matrícula inativa. Procure a recepção.";
            QuickCheckInStatusClass = "danger";
        }
        else
        {
            // Checar se há pagamentos atrasados
            var hasOverdue = await _context.Payments
                .AnyAsync(p => p.StudentId == student.Id && (p.Status == PaymentStatus.Atrasado || (p.Status == PaymentStatus.Pendente && p.DueDate < DateTime.Today)));

            var planExpired = student.PlanEndDate.HasValue && student.PlanEndDate.Value < DateTime.Today;

            if (hasOverdue || planExpired)
            {
                status = AccessStatus.AlertaVencimento;
                message = "Atenção: Mensalidade/Plano vencido! Acesso liberado provisoriamente.";
                QuickCheckInStatusClass = "warning";
            }
            else
            {
                QuickCheckInStatusClass = "success";
            }
        }

        var log = new AttendanceLog
        {
            StudentId = student.Id,
            EntryTime = DateTime.Now,
            AccessStatus = status,
            Message = message
        };

        _context.AttendanceLogs.Add(log);
        await _context.SaveChangesAsync();

        QuickCheckInFeedback = $"{message} ({student.FullName} - {student.RegistrationCode})";
        await LoadDashboardDataAsync();
        return Page();
    }

    private async Task LoadDashboardDataAsync()
    {
        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        TotalActiveStudents = await _context.Students.CountAsync(s => s.IsActive);
        
        CheckInsToday = await _context.AttendanceLogs.CountAsync(a => a.EntryTime >= today);
        
        CurrentlyInGym = await _context.AttendanceLogs.CountAsync(a => a.EntryTime >= today && a.ExitTime == null);

        var monthlyPayments = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Pago && p.PaidDate >= startOfMonth)
            .Select(p => p.Amount)
            .ToListAsync();
        MonthlyRevenue = monthlyPayments.Sum();

        OverduePaymentsCount = await _context.Payments
            .CountAsync(p => p.Status == PaymentStatus.Atrasado || (p.Status == PaymentStatus.Pendente && p.DueDate < today));

        var currentDayOfWeek = DateTime.Today.DayOfWeek;
        var rawClasses = await _context.BoxingClasses
            .Where(c => c.IsActive && c.DayOfWeek == currentDayOfWeek)
            .ToListAsync();
        TodayClasses = rawClasses.OrderBy(c => c.StartTime).ToList();

        RecentAttendances = await _context.AttendanceLogs
            .Include(a => a.Student)
            .OrderByDescending(a => a.EntryTime)
            .Take(6)
            .ToListAsync();

        PendingPayments = await _context.Payments
            .Include(p => p.Student)
            .Where(p => p.Status == PaymentStatus.Atrasado || p.Status == PaymentStatus.Pendente)
            .OrderBy(p => p.DueDate)
            .Take(4)
            .ToListAsync();
    }
}
