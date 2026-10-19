namespace BoxeGymManager.Models;

public enum AccessStatus
{
    Autorizado,
    AlertaVencimento,
    BloqueadoInadimplente,
    BloqueadoInativo
}

public class AttendanceLog
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public DateTime EntryTime { get; set; } = DateTime.Now;
    public DateTime? ExitTime { get; set; }
    public AccessStatus AccessStatus { get; set; } = AccessStatus.Autorizado;
    public string Message { get; set; } = "Entrada autorizada";
}
