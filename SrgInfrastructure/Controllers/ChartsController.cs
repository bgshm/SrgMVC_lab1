using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SrgInfrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartsController : ControllerBase
    {
        private readonly SrgDatabaseContext _context;

        public ChartsController(SrgDatabaseContext context)
        {
            _context = context;
        }

        private record TasksPerMonthByMemberResponseItem(string memberName, int month, int tasksPerMonth);
        private record TasksTotalResponseItem(string memberName, int count);

        [HttpGet("tasksPerMonthByMember")]
        public async Task<JsonResult> GetTasksPerMonthByMemberAsync(CancellationToken cancellationToken)
        {
            var data = await _context.Members
                .Where(m => m.LastTaskDate != null && m.TasksPerMonth != null)
                .Select(m => new TasksPerMonthByMemberResponseItem(
                    m.Name,
                    m.LastTaskDate.Value.Month,
                    m.TasksPerMonth.Value))
                .ToListAsync(cancellationToken);

            return new JsonResult(data);
        }

        [HttpGet("tasksTotal")]
        public async Task<JsonResult> GetTasksTotalAsync(CancellationToken cancellationToken)
        {
            var responseItems = await _context.Members
                .Select(m => new TasksTotalResponseItem(
                    m.Name,
                    m.TasksTotal ?? 0))
                .ToListAsync(cancellationToken);

            return new JsonResult(responseItems);
        }
    }
}
