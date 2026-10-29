using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

using BoxeGymManager.Models;
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

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var student = await _context.Students.FindAsync(id);
        if (student == null) return NotFound();

        Student = student;
        await LoadPlansAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Student.FullName))
        {
            ModelState.AddModelError("", "Nome é obrigatório.");
            await LoadPlansAsync();
            return Page();
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
            return Page();
        }

        var dbStudent = await _context.Students.FindAsync(Student.Id);
        if (dbStudent == null) return NotFound();

        dbStudent.Modality = Student.Modality;
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
}
