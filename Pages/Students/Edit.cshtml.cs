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

        var dbStudent = await _context.Students.FindAsync(Student.Id);
        if (dbStudent == null) return NotFound();

        dbStudent.FullName = Student.FullName.Trim();
        dbStudent.CPF = Student.CPF?.Trim() ?? string.Empty;
        dbStudent.BirthDate = Student.BirthDate;
        dbStudent.Phone = Student.Phone?.Trim() ?? string.Empty;
        dbStudent.Email = Student.Email?.Trim() ?? string.Empty;
        dbStudent.Address = Student.Address?.Trim() ?? string.Empty;

        dbStudent.EmergencyContactName = Student.EmergencyContactName?.Trim() ?? string.Empty;
        dbStudent.EmergencyContactPhone = Student.EmergencyContactPhone?.Trim() ?? string.Empty;

        dbStudent.WeightKg = Student.WeightKg;
        dbStudent.WeightCategory = string.IsNullOrWhiteSpace(Student.WeightCategory) ? "Não especificado" : Student.WeightCategory.Trim();
        dbStudent.ExperienceLevel = Student.ExperienceLevel;
        dbStudent.Stance = string.IsNullOrWhiteSpace(Student.Stance) ? "Destro (Orthodox)" : Student.Stance;
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
