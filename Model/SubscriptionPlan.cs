namespace expense_tracker.Model;

public  class SubscriptionPlan
{
    public int PlanId { get; set; }

    public string PlanName { get; set; } = null!;

    public string SubscriptionInterval { get; set; } = null!;

    public decimal Price { get; set; }

    public int? MaxGoal { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
