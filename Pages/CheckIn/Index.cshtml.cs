using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using BoxeGymManager.Models;

namespace BoxeGymManager.Pages.CheckIn;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public string SearchTerm { get; set; } = string.Empty;

    public List<AttendanceLog> CurrentlyInGymList { get; set; } = new();
    public List<AttendanceLog> TodayLogs { get; set; } = new();

    public string? AlertMessage { get; set; }
    public string? AlertType { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostRegisterEntryAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            AlertMessage = "Informe a matrícula, CPF ou nome do atleta.";
            AlertType = "danger";
            await LoadDataAsync();
            return Page();
        }

        var term = SearchTerm.Trim().ToLower();
        var cleanTerm = term.Replace(".", "").Replace("-", "");

        var student = await _context.Students
            .Include(s => s.Plan)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.RegistrationCode.ToLower() == term 
                                   || s.CPF.Replace(".", "").Replace("-", "") == cleanTerm 
                                   || s.FullName.ToLower().Contains(term));

        if (student == null)
        {
            AlertMessage = $"Nenhum atleta encontrado para '{SearchTerm}'.";
            AlertType = "danger";
            await LoadDataAsync();
            return Page();
        }

        // Verificar se já está com entrada em aberto hoje
        var openSession = await _context.AttendanceLogs
            .Where(a => a.StudentId == student.Id && a.EntryTime >= DateTime.Today && a.ExitTime == null)
            .FirstOrDefaultAsync();

        if (openSession != null)
        {
            AlertMessage = $"O atleta {student.FullName} já está com presença ativa na academia desde {openSession.EntryTime:HH:mm}.";
            AlertType = "info";
            await LoadDataAsync();
            return Page();
        }

        var status = AccessStatus.Autorizado;
        var message = "Entrada autorizada! Bons treinos!";
        AlertType = "success";

        if (!student.IsActive)
        {
            status = AccessStatus.BloqueadoInativo;
            if (student.Status == StudentStatus.Trancado)
            {
                var reasonText = !string.IsNullOrWhiteSpace(student.StatusReason) ? $" (Motivo: {student.StatusReason})" : "";
                message = $"ACESSO NEGADO: Matrícula trancada temporariamente.{reasonText}";
                AlertType = "warning";
            }
            else
            {
                var reasonText = !string.IsNullOrWhiteSpace(student.StatusReason) ? $" (Motivo: {student.StatusReason})" : "";
                message = $"ACESSO NEGADO: Matrícula cancelada/encerrada.{reasonText}";
                AlertType = "danger";
            }
        }
        else
        {
            var hasOverdue = await _context.Payments
                .AnyAsync(p => p.StudentId == student.Id && (p.Status == PaymentStatus.Atrasado || (p.Status == PaymentStatus.Pendente && p.DueDate < DateTime.Today)));

            var planExpired = student.PlanEndDate.HasValue && student.PlanEndDate.Value < DateTime.Today;

            if (hasOverdue || planExpired)
            {
                status = AccessStatus.AlertaVencimento;
                message = "ACESSO COM ALERTA: Mensalidade em atraso ou plano vencido!";
                AlertType = "warning";
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

        AlertMessage = $"{message} - {student.FullName} (Matrícula: {student.RegistrationCode})";
        SearchTerm = string.Empty;
        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostRegisterExitAsync(int logId)
    {
        var log = await _context.AttendanceLogs
            .Include(a => a.Student)
            .FirstOrDefaultAsync(a => a.Id == logId);

        if (log != null && log.ExitTime == null)
        {
            log.ExitTime = DateTime.Now;
            await _context.SaveChangesAsync();
            AlertMessage = $"Saída registrada para {log.Student?.FullName}. Horário: {log.ExitTime:HH:mm}.";
            AlertType = "info";
        }

        await LoadDataAsync();
        return Page();
    }

    private async Task LoadDataAsync()
    {
        var today = DateTime.Today;

        CurrentlyInGymList = await _context.AttendanceLogs
            .Include(a => a.Student)
                .ThenInclude(s => s!.Plan)
            .Where(a => a.EntryTime >= today && a.ExitTime == null)
            .OrderByDescending(a => a.EntryTime)
            .ToListAsync();

        TodayLogs = await _context.AttendanceLogs
            .Include(a => a.Student)
            .Where(a => a.EntryTime >= today)
            .OrderByDescending(a => a.EntryTime)
            .ToListAsync();
    }
}
