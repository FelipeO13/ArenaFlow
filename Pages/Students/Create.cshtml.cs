using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using BoxeGymManager.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
namespace BoxeGymManager.Pages.Students;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public CreateModel(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [BindProperty]
    public Student Student { get; set; } = new();

    [BindProperty]
    public IFormFile? PhotoUpload { get; set; }

    public SelectList PlansList { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        // Gerar código de matrícula sugerido
        var count = await _context.Students.CountAsync();
        Student.RegistrationCode = $"BX-{1001 + count}";
        Student.BirthDate = DateTime.Today.AddYears(-20);
        Student.PlanStartDate = DateTime.Today;

        await LoadPlansAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Student.FullName) || string.IsNullOrWhiteSpace(Student.RegistrationCode))
        {
            ModelState.AddModelError("", "Nome completo e matrícula são obrigatórios.");
            await LoadPlansAsync();
            return Page();
        }

        Student.FullName = Student.FullName.Trim();
        Student.RegistrationCode = Student.RegistrationCode.Trim();
        Student.CPF = Student.CPF?.Trim() ?? string.Empty;
        Student.Phone = Student.Phone?.Trim() ?? string.Empty;
        Student.Email = Student.Email?.Trim() ?? string.Empty;
        Student.Address = Student.Address?.Trim() ?? string.Empty;
        Student.EmergencyContactName = Student.EmergencyContactName?.Trim() ?? string.Empty;
        Student.EmergencyContactPhone = Student.EmergencyContactPhone?.Trim() ?? string.Empty;
        Student.MedicalNotes = Student.MedicalNotes?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(Student.Stance))
        {
            Student.Stance = "Destro (Orthodox)";
        }

        // Handle photo upload
        if (PhotoUpload != null && PhotoUpload.Length > 0)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "students");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(PhotoUpload.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await PhotoUpload.CopyToAsync(stream);
            Student.PhotoPath = Path.Combine("images", "students", fileName).Replace("\\", "/");
        }

        // Definir categoria de peso automaticamente se não preenchida
        if (string.IsNullOrWhiteSpace(Student.WeightCategory))
        {
            Student.WeightCategory = CalculateWeightCategory(Student.WeightKg);
        }
        else
        {
            Student.WeightCategory = Student.WeightCategory.Trim();
        }

        // Se selecionou plano, calcular data de término
        if (Student.PlanId.HasValue)
        {
            var plan = await _context.Plans.FindAsync(Student.PlanId.Value);
            if (plan != null)
            {
                var start = Student.PlanStartDate ?? DateTime.Today;
                Student.PlanEndDate = plan.BillingCycle switch
                {
                    PlanBillingCycle.Trimestral => start.AddMonths(3),
                    PlanBillingCycle.Semestral => start.AddMonths(6),
                    PlanBillingCycle.Anual => start.AddYears(1),
                    _ => start.AddMonths(1)
                };
            }
        }

        Student.CreatedAt = DateTime.Now;
        _context.Students.Add(Student);
        await _context.SaveChangesAsync();

        // Gerar primeira mensalidade pendente se tiver plano
        if (Student.PlanId.HasValue)
        {
            var plan = await _context.Plans.FindAsync(Student.PlanId.Value);
            if (plan != null)
            {
                var payment = new Payment
                {
                    StudentId = Student.Id,
                    Amount = plan.Price,
                    DueDate = DateTime.Today.AddDays(5),
                    Status = PaymentStatus.Pendente,
                    ReferenceMonthYear = $"{DateTime.Today.Month:D2}/{DateTime.Today.Year}",
                    ReceiptCode = $"REC-{DateTime.Today.Year}{DateTime.Today.Month:D2}-{Student.Id:D3}",
                    Notes = $"Primeira mensalidade - Matrícula Plano {plan.Name}"
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
            }
        }

        return RedirectToPage("./Index");
    }

    private async Task LoadPlansAsync()
    {
        var plans = await _context.Plans.Where(p => p.IsActive).ToListAsync();
        PlansList = new SelectList(plans, "Id", "Name");
    }

    private string CalculateWeightCategory(decimal weight)
    {
        if (weight <= 0) return "Não especificado";
        if (weight <= 52m) return "Peso Mosca (até 52kg)";
        if (weight <= 57m) return "Peso Pena (até 57kg)";
        if (weight <= 63m) return "Peso Leve (até 63kg)";
        if (weight <= 69m) return "Peso Meio-Médio (até 69kg)";
        if (weight <= 75m) return "Peso Médio (até 75kg)";
        if (weight <= 81m) return "Peso Meio-Pesado (até 81kg)";
        if (weight <= 91m) return "Peso Pesado (até 91kg)";
        return "Peso Superpesado (+91kg)";
    }
}
