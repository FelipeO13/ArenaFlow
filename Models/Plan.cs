namespace BoxeGymManager.Models;

public enum PlanBillingCycle
{
    Mensal,
    Trimestral,
    Semestral,
    Anual
}

public class Plan
{
    private string _name = string.Empty;
    private string _description = string.Empty;

    public int Id { get; set; }
    public string Name { get => _name; set => _name = value ?? string.Empty; } // ex: "Boxe Fit - 2x por Semana", "Nobre Arte Livre (Diário)"
    public string Description { get => _description; set => _description = value ?? string.Empty; }
    public decimal Price { get; set; }
    public PlanBillingCycle BillingCycle { get; set; } = PlanBillingCycle.Mensal;
    public int DaysPerWeek { get; set; } = 3; // 0 = Livre / Ilimitado
    public bool IncludesSparring { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public ICollection<Student> Students { get; set; } = new List<Student>();
}
