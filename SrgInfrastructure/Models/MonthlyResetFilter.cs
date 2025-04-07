// MonthlyResetFilter.cs
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SrgDomain.Model;
using SrgInfrastructure;

public class MonthlyResetFilter : IAsyncActionFilter
{
    private readonly SrgDatabaseContext _db;
    public MonthlyResetFilter(SrgDatabaseContext db) => _db = db;

    public async System.Threading.Tasks.Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var now = DateTime.Now;

        // 1) Reset TasksPerMonth for members whose LastTaskDate was in a previous month
        var toReset = await _db.Members
            .Where(m => m.LastTaskDate.HasValue && m.LastTaskDate.Value.Month != now.Month && m.TasksPerMonth != 0)
            .ToListAsync();
        foreach (var m in toReset)
        {
            m.TasksPerMonth = 0;
        }

        // 2) Mark overdue tasks (deadline passed and not completed)
        var overdue = await _db.Tasks
            .Where(t =>
                t.Deadline.HasValue &&
                t.Deadline < now &&
                t.Status != "Completed" &&             // simple != comparison
                !t.Status.Contains("overdue")           // simple Contains
            )
            .ToListAsync();
        foreach (var t in overdue)
        {
            t.Status = "In Progress (overdue)";
        }

        await _db.SaveChangesAsync();

        await next();
    }
}
