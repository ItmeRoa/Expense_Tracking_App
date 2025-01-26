
namespace expense_tracker.Model;

public  class TransactionType
{
    public int TypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
