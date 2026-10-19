namespace BoxeGymManager.Models;

public class BoxingClass
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty; // ex: "Fundamentos & Técnica", "Sparring Supervisionado"
    public string CoachName { get; set; } = string.Empty; // ex: "Mestre Popó", "Prof. Anderson"
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int MaxCapacity { get; set; } = 15;
    public string LevelRequirement { get; set; } = "Todos os níveis"; // Iniciante / Avançado / Sparring
    public string RingOrRoom { get; set; } = "Ringue Principal"; // Ringue 1, Sala de Sacos, Tatame
    public bool IsActive { get; set; } = true;
}
