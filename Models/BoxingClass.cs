namespace BoxeGymManager.Models;

public class BoxingClass
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public SportModality Modality { get; set; } = SportModality.Boxe;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string CoachName { get; set; } = string.Empty; // Instrutor
    public int MaxCapacity { get; set; } = 15; // Limite de alunos
    public string? Description { get; set; } // Opcional, texto curto

    // Campos legados mantidos no banco para compatibilidade, sem uso na interface
    public string Title { get; set; } = string.Empty;
    public string LevelRequirement { get; set; } = "Todos os níveis";
    public string RingOrRoom { get; set; } = "Ringue Principal";
    public bool IsActive { get; set; } = true;

    public ICollection<Student> Students { get; set; } = new List<Student>();

    // Nome exibido da aula gerado automaticamente: "Boxe 07:00 às 08:00"
    public string ModalityName => Modality switch
    {
        SportModality.JiuJitsu => "Jiu-Jitsu",
        SportModality.MuayThai => "Muay Thai",
        _ => "Boxe"
    };

    public string DisplayName => $"{ModalityName} {StartTime:hh\\:mm} às {EndTime:hh\\:mm}";
}
