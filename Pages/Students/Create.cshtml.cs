using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoxeGymManager.Data;
using BoxeGymManager.Models;
using BoxeGymManager.Services;
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

    public int[] DiasVencimentoPermitidos => CalculadoraVencimento.DiasVencimentoPermitidos;

    public async Task<IActionResult> OnGetAsync()
    {
        // Gerar código de matrícula sugerido
        var count = await _context.Students.CountAsync();
        Student.RegistrationCode = $"BX-{1001 + count}";
        Student.BirthDate = DateTime.Today.AddYears(-20);
        Student.PlanStartDate = DateTime.Today;
        Student.DiaVencimento = 10; // Padrão selecionado

        await LoadPlansAsync();
        return Page();
    }

    public async Task<IActionResult> OnGetCalcularProporcionalAsync(int planId, int diaVencimento, string? dataMatricula)
    {
        var plan = await _context.Plans.FindAsync(planId);
        if (plan == null) return NotFound();

        DateTime start = DateTime.TryParse(dataMatricula, out var d) ? d : DateTime.Today;
        var res = CalculadoraVencimento.CalcularPrimeiroVencimento(plan.Price, diaVencimento, start);

        return new JsonResult(new
        {
            valorMensalidade = res.ValorMensalidade,
            valorDiario = res.ValorDiario,
            dataReferencia = res.DataReferencia.ToString("dd/MM/yyyy"),
            primeiroVencimento = res.PrimeiroVencimento.ToString("dd/MM/yyyy"),
            diferencaDias = res.DiferencaDias,
            valorAjuste = res.ValorAjuste,
            valorPrimeiroPagamento = res.ValorPrimeiroPagamento,
            tipoAjuste = res.DiferencaDias < 0 ? "desconto" : (res.DiferencaDias > 0 ? "acrescimo" : "nenhum")
        });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Student.FullName) || string.IsNullOrWhiteSpace(Student.RegistrationCode))
        {
            ModelState.AddModelError("", "Nome completo e matrícula são obrigatórios.");
            await LoadPlansAsync();
            return Page();
        }

        // Validação de Dia de Vencimento
        if (Student.PlanId.HasValue)
        {
            if (!Student.DiaVencimento.HasValue || !CalculadoraVencimento.DiasVencimentoPermitidos.Contains(Student.DiaVencimento.Value))
            {
                ModelState.AddModelError("Student.DiaVencimento", "Selecione o dia de vencimento fixo (10 ou 25).");
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
            return Page();
        }

        Student.FullName = Student.FullName.Trim();
        Student.RegistrationCode = Student.RegistrationCode.Trim();
        Student.CPF = Student.CPF?.Trim() ?? string.Empty;
        Student.Phone = Student.Phone?.Trim() ?? string.Empty;
        Student.Email = Student.Email?.Trim() ?? string.Empty;
        Student.Address = Student.Address?.Trim() ?? string.Empty;
        Student.GuardianName = string.IsNullOrWhiteSpace(Student.GuardianName) ? null : Student.GuardianName.Trim();
        Student.GuardianPhone = string.IsNullOrWhiteSpace(Student.GuardianPhone) ? null : Student.GuardianPhone.Trim();
        Student.GuardianAddress = string.IsNullOrWhiteSpace(Student.GuardianAddress) ? null : Student.GuardianAddress.Trim();
        Student.ModalityNotes = string.IsNullOrWhiteSpace(Student.ModalityNotes) ? null : Student.ModalityNotes.Trim();
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

        // Se selecionou plano, calcular data de término inicial
        if (Student.PlanId.HasValue)
        {
            var plan = await _context.Plans.FindAsync(Student.PlanId.Value);
            if (plan != null)
            {
                var start = Student.PlanStartDate ?? DateTime.Today;
                if (Student.DiaVencimento.HasValue)
                {
                    var res = CalculadoraVencimento.CalcularPrimeiroVencimento(plan.Price, Student.DiaVencimento.Value, start);
                    Student.PlanEndDate = res.PrimeiroVencimento;
                }
                else
                {
                    Student.PlanEndDate = plan.BillingCycle switch
                    {
                        PlanBillingCycle.Trimestral => start.AddMonths(3),
                        PlanBillingCycle.Semestral => start.AddMonths(6),
                        PlanBillingCycle.Anual => start.AddYears(1),
                        _ => start.AddMonths(1)
                    };
                }
            }
        }

        Student.CreatedAt = DateTime.Now;
        _context.Students.Add(Student);
        await _context.SaveChangesAsync();

        // Gerar primeira mensalidade se tiver plano
        if (Student.PlanId.HasValue)
        {
            var plan = await _context.Plans.FindAsync(Student.PlanId.Value);
            if (plan != null)
            {
                var start = Student.PlanStartDate ?? DateTime.Today;
                decimal valorPrimeiroPagamento = plan.Price;
                DateTime vencimentoPrimeiroPagamento = DateTime.Today.AddDays(5);
                string obsAjuste = "Primeira mensalidade";

                if (Student.DiaVencimento.HasValue)
                {
                    var res = CalculadoraVencimento.CalcularPrimeiroVencimento(plan.Price, Student.DiaVencimento.Value, start);
                    valorPrimeiroPagamento = res.ValorPrimeiroPagamento;
                    vencimentoPrimeiroPagamento = res.PrimeiroVencimento;
                    var sinal = res.DiferencaDias > 0 ? $"+{res.DiferencaDias}" : $"{res.DiferencaDias}";
                    var tipo = res.DiferencaDias < 0 ? $"desconto de {Math.Abs(res.ValorAjuste):C}" : (res.DiferencaDias > 0 ? $"acréscimo de {res.ValorAjuste:C}" : "sem ajuste");
                    obsAjuste = $"1ª Mensalidade proporcional ({sinal} dias, {tipo}) - Venc. dia {Student.DiaVencimento}";
                }

                var payment = new Payment
                {
                    StudentId = Student.Id,
                    Amount = valorPrimeiroPagamento,
                    DueDate = vencimentoPrimeiroPagamento,
                    Status = PaymentStatus.Pendente,
                    ReferenceMonthYear = $"{vencimentoPrimeiroPagamento.Month:D2}/{vencimentoPrimeiroPagamento.Year}",
                    ReceiptCode = $"REC-{DateTime.Today.Year}{DateTime.Today.Month:D2}-{Student.Id:D3}",
                    Notes = $"{obsAjuste} - Plano {plan.Name}"
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
