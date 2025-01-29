namespace expense_tracker.Model;

public class Transaction
{
    public int TransactionId { get; set; }

    public string TransactionName { get; set; } = null!;

    public int TypeId { get; set; }

    public DateTime TransactionDate { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = null!;

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual TransactionType Type { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}