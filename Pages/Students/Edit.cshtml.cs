using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

using BoxeGymManager.Models;
using BoxeGymManager.Services;

namespace BoxeGymManager.Pages.Students;

public class EditModel : PageModel
{
    private readonly AppDbContext _context;

    public EditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Student Student { get; set; } = null!;

    public SelectList PlansList { get; set; } = null!;
    public List<SelectListItem> AvailableClasses { get; set; } = new();

    public int[] DiasVencimentoPermitidos => CalculadoraVencimento.DiasVencimentoPermitidos;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var student = await _context.Students.FindAsync(id);
        if (student == null) return NotFound();

        Student = student;
        await LoadPlansAsync();
        await LoadClassesAsync(Student.Modality, Student.BoxingClassId, Student.Id);
        return Page();
    }

    public async Task<IActionResult> OnGetClassesByModalityAsync(SportModality modality, int? currentStudentId)
    {
        var rawClasses = await _context.BoxingClasses
            .Include(c => c.Students)
            .Where(c => c.IsActive && c.Modality == modality)
            .ToListAsync();

        var ordered = rawClasses
            .OrderBy(c => ScheduleHelper.GetDayOrder(c.DayOfWeek))
            .ThenBy(c => c.StartTime)
            .Select(c => ScheduleHelper.ToOptionDto(c, currentStudentId))
            .ToList();

        return new JsonResult(ordered);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Student.FullName))
        {
            ModelState.AddModelError("", "Nome é obrigatório.");
        }

        // Validação da aula alocada
        if (Student.BoxingClassId.HasValue && Student.BoxingClassId.Value > 0)
        {
            var selectedClass = await _context.BoxingClasses
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == Student.BoxingClassId.Value && c.IsActive);

            if (selectedClass == null)
            {
                ModelState.AddModelError("Student.BoxingClassId", "A aula selecionada não foi encontrada ou não está ativa.");
            }
            else if (selectedClass.Modality != Student.Modality)
            {
                ModelState.AddModelError("Student.BoxingClassId", "A aula selecionada não pertence à modalidade escolhida.");
            }
            else
            {
                // Verifica se mudou de aula e se a nova está cheia
                var dbCurrentClassId = await _context.Students
                    .Where(s => s.Id == Student.Id)
                    .Select(s => s.BoxingClassId)
                    .FirstOrDefaultAsync();

                if (dbCurrentClassId != Student.BoxingClassId.Value)
                {
                    var occupiedOthers = selectedClass.Students.Count(s => s.Id != Student.Id);
                    if (occupiedOthers >= selectedClass.MaxCapacity)
                    {
                        ModelState.AddModelError("Student.BoxingClassId", "A aula selecionada está lotada. Escolha outro horário com vagas disponíveis.");
                    }
                }
            }
        }

        // Validação de CNH (opcional, mas deve ter 11 dígitos se preenchida)
        if (!string.IsNullOrWhiteSpace(Student.CNH))
        {
            var cnhDigits = System.Text.RegularExpressions.Regex.Replace(Student.CNH.Trim(), @"\D", "");
            if (cnhDigits.Length != 11)
            {
                ModelState.AddModelError("Student.CNH", "A CNH deve conter exatamente 11 dígitos numéricos.");
            }
            else
            {
                Student.CNH = cnhDigits;
            }
        }
        else
        {
            Student.CNH = null;
        }

        // Validação de Filiação para menores de 18 anos
        if (Student.IsUnderage)
        {
            if (string.IsNullOrWhiteSpace(Student.GuardianName))
            {
                ModelState.AddModelError("Student.GuardianName", "Nome do pai, mãe ou responsável é obrigatório para menores de 18 anos.");
            }
            if (string.IsNullOrWhiteSpace(Student.GuardianPhone))
            {
                ModelState.AddModelError("Student.GuardianPhone", "Telefone do responsável é obrigatório para menores de 18 anos.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadPlansAsync();
            await LoadClassesAsync(Student.Modality, Student.BoxingClassId, Student.Id);
            return Page();
        }

        var dbStudent = await _context.Students.FindAsync(Student.Id);
        if (dbStudent == null) return NotFound();

        dbStudent.Modality = Student.Modality;
        dbStudent.BoxingClassId = (Student.BoxingClassId.HasValue && Student.BoxingClassId.Value > 0)
            ? Student.BoxingClassId.Value
            : null;
        dbStudent.FullName = Student.FullName.Trim();
        dbStudent.CPF = Student.CPF?.Trim() ?? string.Empty;
        dbStudent.BirthDate = Student.BirthDate;
        dbStudent.CNH = string.IsNullOrWhiteSpace(Student.CNH) ? null : Student.CNH.Trim();
        dbStudent.GuardianName = string.IsNullOrWhiteSpace(Student.GuardianName) ? null : Student.GuardianName.Trim();
        dbStudent.GuardianPhone = string.IsNullOrWhiteSpace(Student.GuardianPhone) ? null : Student.GuardianPhone.Trim();
        dbStudent.GuardianAddress = string.IsNullOrWhiteSpace(Student.GuardianAddress) ? null : Student.GuardianAddress.Trim();
        dbStudent.Phone = Student.Phone?.Trim() ?? string.Empty;
        dbStudent.Email = Student.Email?.Trim() ?? string.Empty;
        dbStudent.Address = Student.Address?.Trim() ?? string.Empty;

        dbStudent.EmergencyContactName = Student.EmergencyContactName?.Trim() ?? string.Empty;
        dbStudent.EmergencyContactPhone = Student.EmergencyContactPhone?.Trim() ?? string.Empty;

        dbStudent.WeightKg = Student.WeightKg;
        dbStudent.WeightCategory = string.IsNullOrWhiteSpace(Student.WeightCategory) ? "Não especificado" : Student.WeightCategory.Trim();
        dbStudent.ExperienceLevel = Student.ExperienceLevel;
        dbStudent.Stance = string.IsNullOrWhiteSpace(Student.Stance) ? "Destro (Orthodox)" : Student.Stance;
        dbStudent.HeightCm = Student.HeightCm;
        dbStudent.ReachCm = Student.ReachCm;
        dbStudent.ModalityNotes = string.IsNullOrWhiteSpace(Student.ModalityNotes) ? null : Student.ModalityNotes.Trim();
        dbStudent.MedicalNotes = Student.MedicalNotes?.Trim() ?? string.Empty;
        dbStudent.HasMedicalCertificate = Student.HasMedicalCertificate;

        dbStudent.PlanId = Student.PlanId;
        dbStudent.PlanStartDate = Student.PlanStartDate;
        dbStudent.PlanEndDate = Student.PlanEndDate;
        dbStudent.DiaVencimento = Student.DiaVencimento;
        if (dbStudent.Status != Student.Status)
        {
            dbStudent.Status = Student.Status;
            dbStudent.StatusChangedAt = DateTime.Now;
        }
        dbStudent.StatusReason = Student.StatusReason?.Trim() ?? string.Empty;
        dbStudent.IsActive = Student.Status == StudentStatus.Ativo;

        await _context.SaveChangesAsync();
        return RedirectToPage("./Details", new { id = Student.Id });
    }

    private async Task LoadPlansAsync()
    {
        var plans = await _context.Plans.Where(p => p.IsActive).ToListAsync();
        PlansList = new SelectList(plans, "Id", "Name");
    }

    private async Task LoadClassesAsync(SportModality modality, int? currentClassId, int? currentStudentId)
    {
        var rawClasses = await _context.BoxingClasses
            .Include(c => c.Students)
            .Where(c => c.IsActive && c.Modality == modality)
            .ToListAsync();

        var ordered = rawClasses
            .OrderBy(c => ScheduleHelper.GetDayOrder(c.DayOfWeek))
            .ThenBy(c => c.StartTime)
            .Select(c => ScheduleHelper.ToOptionDto(c, currentStudentId))
            .ToList();

        AvailableClasses = ordered.Select(dto => new SelectListItem
        {
            Value = dto.Id.ToString(),
            Text = dto.DisplayText,
            Disabled = dto.IsFull,
            Selected = currentClassId == dto.Id
        }).ToList();
    }
}
