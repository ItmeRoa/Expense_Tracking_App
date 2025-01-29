namespace expense_tracker.Model;

public class Goal
{
    public int GoalId { get; set; }

    public int UserId { get; set; }

    public int TypeId { get; set; }

    public decimal TargetAmount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string GoalStatus { get; set; } = null!;

    public virtual TransactionType Type { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}