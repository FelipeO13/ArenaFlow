namespace BoxeGymManager.Models;

public enum PaymentStatus
{
    Pendente,
    Pago,
    Atrasado,
    Cancelado
}

public enum PaymentMethod
{
    Pix,
    Dinheiro,
    CartaoCredito,
    CartaoDebito,
    Boleto
}

public class Payment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pendente;
    public PaymentMethod Method { get; set; } = PaymentMethod.Pix;

    private string _referenceMonthYear = string.Empty;
    private string _receiptCode = string.Empty;
    private string _notes = string.Empty;

    public string ReferenceMonthYear { get => _referenceMonthYear; set => _referenceMonthYear = value ?? string.Empty; } // ex: "10/2026"
    public string ReceiptCode { get => _receiptCode; set => _receiptCode = value ?? string.Empty; } // ex: "REC-202610-001"
    public string Notes { get => _notes; set => _notes = value ?? string.Empty; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
