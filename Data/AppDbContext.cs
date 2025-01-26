using expense_tracker.Model;
using Microsoft.EntityFrameworkCore;

namespace expense_tracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Goal> Goals { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionType> TransactionTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAuth> UserAuths { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__D54EE9B46BA88CFA");

            entity.ToTable("Categories", "track");

            entity.HasIndex(e => e.CategoryName, "UQ__Categori__5189E25516600F2B").IsUnique();

            entity.HasIndex(e => new { e.UserId, e.CategoryName }, "UQ__Categori__ECA6A92BACA5D36C").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("category_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Categories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Categorie__user___6B24EA82");
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.GoalId).HasName("PK__Goals__76679A24AAF4C124");

            entity.ToTable("Goals", "track");

            entity.HasIndex(e => new { e.UserId, e.TypeId }, "IX_Goals_User_Type");

            entity.Property(e => e.GoalId).HasColumnName("goal_id");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.GoalStatus)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("active")
                .HasColumnName("goal_status");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.TargetAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("target_amount");
            entity.Property(e => e.TypeId).HasColumnName("type_Id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Type).WithMany(p => p.Goals)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Goals__type_Id__797309D9");

            entity.HasOne(d => d.User).WithMany(p => p.Goals)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Goals__user_id__787EE5A0");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__Subscrip__863A7EC1B82F1035");

            entity.ToTable("Subscriptions", "subscription");

            entity.HasIndex(e => new { e.UserId, e.PlanId }, "IX_Active_Subscriptions")
                .HasFilter("([subscription_status]='active')");

            entity.HasIndex(e => e.EndDate, "IX_Subscriptions_EndDate");

            entity.HasIndex(e => e.NextBillingDate, "IX_Subscriptions_NextBillingDate");

            entity.HasIndex(e => e.SubscriptionStatus, "IX_Subscriptions_Status");

            entity.HasIndex(e => e.UserId, "IX_Subscriptions_UserID");

            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.NextBillingDate)
                .HasColumnType("datetime")
                .HasColumnName("next_billing_date");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.SubscriptionStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active")
                .HasColumnName("subscription_status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Subscript__plan___628FA481");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Subscript__user___619B8048");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__Subscrip__BE9F8F1DC7466426");

            entity.ToTable("Subscription_Plans", "subscription");

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MaxGoal).HasColumnName("max_goal");
            entity.Property(e => e.PlanName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("plan_name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.SubscriptionInterval)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("subscription_interval");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__85C600AFE80BDDEA");

            entity.ToTable("Transactions", "track");

            entity.HasIndex(e => e.TransactionDate, "IX_Transactions_Date");

            entity.HasIndex(e => e.UserId, "IX_Transactions_UserID");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CategoryId).HasColumnName("category_Id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.TransactionDate)
                .HasColumnType("datetime")
                .HasColumnName("transaction_date");
            entity.Property(e => e.TransactionName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("transaction_name");
            entity.Property(e => e.TypeId).HasColumnName("type_Id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__categ__75A278F5");

            entity.HasOne(d => d.Type).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__type___71D1E811");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Transacti__user___74AE54BC");
        });

        modelBuilder.Entity<TransactionType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__Transact__2C00059801A8DA0A");

            entity.ToTable("Transaction_Types", "track");

            entity.HasIndex(e => e.TypeName, "UQ__Transact__543C4FD9E5811E26").IsUnique();

            entity.Property(e => e.TypeId).HasColumnName("type_id");
            entity.Property(e => e.TypeName)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370FA0A9768C");

            entity.ToTable("Users", "auth");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E616415086F19").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("last_name");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("middle_name");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<UserAuth>(entity =>
        {
            entity.HasKey(e => e.UserAuthId).HasName("PK__User_Aut__2E5F16D15FBFB265");

            entity.ToTable("User_Auths", "auth");

            entity.Property(e => e.UserAuthId).HasColumnName("user_auth_id");
            entity.Property(e => e.AuthProvider)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("auth_provider");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PasswordHashed)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hashed");
            entity.Property(e => e.ProviderUserId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("provider_user_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserAuths)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__User_Auth__user___5CD6CB2B");
        });
    }
}