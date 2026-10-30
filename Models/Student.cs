using System.ComponentModel.DataAnnotations;

namespace BoxeGymManager.Models;

public enum SportModality
{
    [Display(Name = "Boxe")]
    Boxe = 0,

    [Display(Name = "Jiu-Jitsu")]
    JiuJitsu = 1,

    [Display(Name = "Muay Thai")]
    MuayThai = 2
}

public enum BoxingLevel
{
    Iniciante,
    Amador,
    Intermediario,
    Sparring,
    CompetidorProfissional
}

public enum StudentStatus
{
    Ativo,
    Trancado,
    Cancelado
}

public class Student
{
    private string _registrationCode = string.Empty;
    private string _fullName = string.Empty;
    private string _cpf = string.Empty;
    private string _phone = string.Empty;
    private string _email = string.Empty;
    private string _address = string.Empty;
    private string _emergencyContactName = string.Empty;
    private string _emergencyContactPhone = string.Empty;
    private string _weightCategory = string.Empty;
    private string _stance = "Destro (Orthodox)";
    private string _medicalNotes = string.Empty;

    public int Id { get; set; }

    // Modalidade Esportiva
    public SportModality Modality { get; set; } = SportModality.Boxe;

    public string RegistrationCode { get => _registrationCode; set => _registrationCode = value ?? string.Empty; }
    public string FullName { get => _fullName; set => _fullName = value ?? string.Empty; }
    public string CPF { get => _cpf; set => _cpf = value ?? string.Empty; }
    public DateTime BirthDate { get; set; }
    public string? CNH { get; set; }

    // Filiação / Responsável Legal (obrigatório se menor de 18 anos)
    public string? GuardianName { get; set; }
    public string? GuardianPhone { get; set; }
    public string? GuardianAddress { get; set; }

    public string Phone { get => _phone; set => _phone = value ?? string.Empty; }
    public string Email { get => _email; set => _email = value ?? string.Empty; }
    public string Address { get => _address; set => _address = value ?? string.Empty; }

    // Cálculo de idade
    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - BirthDate.Year;
            if (BirthDate.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    public bool IsUnderage => Age < 18;

    // Ficha de Contato de Emergência
    public string EmergencyContactName { get => _emergencyContactName; set => _emergencyContactName = value ?? string.Empty; }
    public string EmergencyContactPhone { get => _emergencyContactPhone; set => _emergencyContactPhone = value ?? string.Empty; }

    // Ficha Técnica Esportiva
    public decimal WeightKg { get; set; }
    public string WeightCategory { get => _weightCategory; set => _weightCategory = value ?? string.Empty; }
    public BoxingLevel ExperienceLevel { get; set; } = BoxingLevel.Iniciante;
    public string Stance { get => _stance; set => _stance = string.IsNullOrWhiteSpace(value) ? "Destro (Orthodox)" : value; }
    public int? HeightCm { get; set; }
    public int? ReachCm { get; set; }
    public string? ModalityNotes { get; set; }
    public string MedicalNotes { get => _medicalNotes; set => _medicalNotes = value ?? string.Empty; }
    public bool HasMedicalCertificate { get; set; } = true;
    public string? PhotoPath { get; set; }

    // Plano e Matrícula
    public int? PlanId { get; set; }
    public Plan? Plan { get; set; }
    public DateTime? PlanStartDate { get; set; }
    public DateTime? PlanEndDate { get; set; }
    public int? DiaVencimento { get; set; }

    private string _statusReason = string.Empty;

    // Situação da Matrícula
    public StudentStatus Status { get; set; } = StudentStatus.Ativo;
    public string StatusReason { get => _statusReason; set => _statusReason = value ?? string.Empty; }
    public DateTime? StatusChangedAt { get; set; }

    // Compatibilidade com verificações existentes
    public bool IsActive
    {
        get => Status == StudentStatus.Ativo;
        set
        {
            if (value && Status != StudentStatus.Ativo) Status = StudentStatus.Ativo;
            else if (!value && Status == StudentStatus.Ativo) Status = StudentStatus.Cancelado;
        }
    }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();
}
