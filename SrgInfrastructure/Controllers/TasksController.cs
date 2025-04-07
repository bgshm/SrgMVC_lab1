using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SrgDomain.Model;
using System.Security.Claims;

namespace SrgInfrastructure.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly SrgDatabaseContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(SrgDatabaseContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tasks – filter tasks by the department (via the manager).
        [Authorize(Roles = "Member,Manager,Admin")]
        public async Task<IActionResult> Index()
        {
            IQueryable<SrgDomain.Model.Task> query = _context.Tasks.Include(t => t.Manager);
            if (User.IsInRole("Member") || User.IsInRole("Manager"))
            {
                var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user != null && user.DepartmentId.HasValue)
                {
                    query = query.Where(t => t.Manager.DepartmentId == user.DepartmentId.Value);
                }
                else
                {
                    query = query.Where(t => false);
                }
            }
            return View(await query.ToListAsync());
        }

        // GET: Tasks/AttachedMembers/5 – all roles.
        [Authorize(Roles = "Member,Manager,Admin")]
        public async Task<IActionResult> AttachedMembers(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            return View(task);
        }

        // GET: Tasks/Details/5 – filter similarly.
        [Authorize(Roles = "Member,Manager,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            IQueryable<SrgDomain.Model.Task> query = _context.Tasks
                .Include(t => t.Manager)
                .Include(t => t.TaskHistories);
            if (User.IsInRole("Member") || User.IsInRole("Manager"))
            {
                var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user != null && user.DepartmentId.HasValue)
                {
                    query = query.Where(t => t.Manager.DepartmentId == user.DepartmentId.Value);
                }
                else
                {
                    return NotFound();
                }
            }
            var task = await query.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
                return NotFound();

            return View(task);
        }

        // GET: Tasks/Create?managerId=5 – only Managers and Admins.
        [Authorize(Roles = "Manager,Admin")]
        public IActionResult Create(int managerId)
        {
            ViewBag.ManagerId = managerId;
            ViewBag.AvailableMembers = _context.Members.ToList();
            return View();
        }

        // POST: Tasks/Create – only Managers and Admins.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Create(SrgDomain.Model.Task task, int[] SelectedMemberIds)
        {
            // Associate the task with its manager.
            task.Manager = _context.Managers.FirstOrDefault(m => m.Id == task.ManagerId);

            // Add selected members.
            if (SelectedMemberIds != null)
            {
                foreach (var memberId in SelectedMemberIds)
                {
                    var member = _context.Members.Find(memberId);
                    if (member != null)
                    {
                        task.Members.Add(member);
                    }
                }
            }

            _context.Add(task);
            await _context.SaveChangesAsync();

            var managerName = "Адміністратор";
            if (!User.IsInRole("Admin"))
                managerName = task.Manager?.Name;

            // Log history entry for task creation.
            await LogTaskHistory(task.Id, $"Завдання {task.Title} було створено користувачем {managerName}");

            return RedirectToAction("Details", "Managers", new { id = task.ManagerId });
        }

        // GET: Tasks/Edit/5 – only Managers and Admins.
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            return View(task);
        }

        // POST: Tasks/Edit/5 – only Managers and Admins.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Edit(int id, SrgDomain.Model.Task task)
        {
            if (id != task.Id) return NotFound();

            task.Manager = _context.Managers.FirstOrDefault(m => m.Id == task.ManagerId);
            _context.Update(task);
            await _context.SaveChangesAsync();

            var managerName = "Адміністратор";
            if (!User.IsInRole("Admin"))
                managerName = task.Manager?.Name;
            // Log history entry for task modification.
            await LogTaskHistory(task.Id, $"Завдання {task.Title} було змінено користувачем {managerName}");

            return RedirectToAction("Details", "Managers", new { id = task.ManagerId });
        }

        // GET: Tasks/Delete/5 – only Managers and Admins.
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks
                .Include(t => t.Manager)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();

            return View(task);
        }

        // POST: Tasks/Delete/5 – only Managers and Admins.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                var managerName = "Адміністратор";
                if (!User.IsInRole("Admin"))
                    managerName = task.Manager?.Name;
                // Log the deletion event before removing the task.
                await LogTaskHistory(task.Id, $"Завдання \"{task.Title}\" було видалено користувачем {managerName}");

                int managerId = task.ManagerId;
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Managers", new { id = managerId });
            }
            return RedirectToAction("Index", "Managers");
        }

        // POST: Tasks/MarkAsCompleted/5 – only Managers and Admins.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> MarkAsCompleted(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            if (!string.Equals(task.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                task.Status = "Completed";
                _context.Update(task);
                await _context.SaveChangesAsync();

                var managerName = "Адміністратор";
                if (!User.IsInRole("Admin"))
                    managerName = task.Manager?.Name;
                // Log history entry for marking task as completed.
                await LogTaskHistory(task.Id, $"Завдання {task.Title} було позначено як виконане користувачем {managerName}");
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper method to log task history.
        private async System.Threading.Tasks.Task LogTaskHistory(int taskId, string description)
        {
            var history = new TaskHistory
            {
                TaskId = taskId,
                Action = description,
                Timestamp = DateTime.Now
            };

            _context.TaskHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
