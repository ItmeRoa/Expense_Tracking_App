using System.Data.Common;
using expense_tracker.Data;
using expense_tracker.Exception;
using expense_tracker.Model;
using expense_tracker.Repo.IRepo;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace expense_tracker.Repo;

public class UserRepo : IUserRepo
{
    private readonly AppDbContext _appDbContext;
    private readonly ILogger _logger;

    public UserRepo(AppDbContext appDbContext, ILogger logger)
    {
        _appDbContext = appDbContext;
        _logger = logger;
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        try
        {
            return await _appDbContext.Users
                .Include(u => u.UserAuths)
                .Include(u => u.Subscriptions).ThenInclude(s => s.Plan)
                .FirstOrDefaultAsync(u => u.Email == email) ?? throw new EntityNotFoundException(typeof(User), email);
        }
        catch (EntityNotFoundException e)
        {
            _logger.Error(e, "The user does not exist");
            throw;
        }
        catch (DbException ex)
        {
            _logger.Error(ex, "Unexpected error occurred while getting a user by email.");
            throw new RepoException("Error getting the user by the email", ex);
        }
    }

    public async Task<bool> UserDoesExistByEmailAsync(string email)
    {
        return await _appDbContext.Users.AsNoTracking().AnyAsync(u => u.Email == email);
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        try
        {
            return await _appDbContext.Users.Include(u => u.Subscriptions).ThenInclude(s => s.Plan)
                .FirstAsync(u => u.UserId == id) ?? throw new EntityNotFoundException(typeof(User), id);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.Error(ex, "The user does not exist");
            throw;
        }
        catch (DbException ex)
        {
            _logger.Error(ex, "Unexpected error occurred while getting a user by id.");
            throw new RepoException("Error getting the user by the id", ex);
        }
    }

    public Task<SubscriptionPlan> GetSubscriptionPlanByNameAndIntervalAsync(string planName, string interval)
    {
        throw new NotImplementedException();
    }

    public async Task<Subscription> SetBasicPlanToUserAsync(User user)
    {
        SubscriptionPlan subscriptionPlan = await _appDbContext.SubscriptionPlans.Where(sp => sp.PlanName == "Basic")
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync() ??
                                            throw new EntityNotFoundException(typeof(SubscriptionPlan),
                                                "The basic plan does not exist ");

        return new Subscription
        {
            PlanId = subscriptionPlan.PlanId,
            UserId = user.UserId,
        };
    }


    public async Task<User> CreateUserAsync(User user, UserAuth userAuth)
    {
        await using var transaction = await _appDbContext.Database.BeginTransactionAsync();

        try
        {
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync(); // So SQLServer can create a userId for us
            // since the user is already tracked by EF core we don't need to query the user again for the userId

            userAuth.UserId = user.UserId;
            await _appDbContext.UserAuths.AddAsync(userAuth);
            await _appDbContext.SaveChangesAsync();

            Subscription subscription = await SetBasicPlanToUserAsync(user);
            await _appDbContext.Subscriptions.AddAsync(subscription);
            await _appDbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return user;
        }
        catch (DbException ex)
        {
            await transaction.RollbackAsync();
            _logger.Error(ex,
                $"Unexpected error occurred while creating an account for the user with email {user.Email}.");
            throw new RepoException($"Error creating an account for the user{user.Email}", ex);
        }
    }

    public async Task VerifiedEmail(User user)
    {
        try
        {
            _logger.Information("Attempting to update the user status");
            user.IsEmailVerified = true;
            await _appDbContext.SaveChangesAsync();
            _logger.Information("Successfully updating the user");
        }
        catch (DbException ex)
        {
            throw new RepoException("Error during updating role in repo layer", ex);
        }
    }

    public Task<User> UpdateUserAsync(int id, User user)
    {
        throw new NotImplementedException();
    }

    public Task DeleteUserAsync(int id)
    {
        throw new NotImplementedException();
    }
}