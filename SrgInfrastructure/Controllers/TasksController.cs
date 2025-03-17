using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SrgDomain.Model;
using SrgInfrastructure;

namespace SrgInfrastructure.Controllers
{
    public class TasksController : Controller
    {
        private readonly SrgDatabaseContext _context;

        public TasksController(SrgDatabaseContext context)
        {
            _context = context;
        }

        // GET: Tasks
        public async Task<IActionResult> Index()
        {
            var srgDatabaseContext = _context.Tasks.Include(t => t.Manager);
            return View(await srgDatabaseContext.ToListAsync());
        }

        // GET: Tasks/AttachedMembers/5
        public async Task<IActionResult> AttachedMembers(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            return View(task);
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks
                .Include(t => t.Manager)
                .Include(t => t.TaskHistories)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();

            return View(task);
        }

        // GET: Tasks/Create?managerId=5
        public IActionResult Create(int managerId)
        {
            ViewBag.ManagerId = managerId;
            ViewBag.AvailableMembers = _context.Members.ToList();
            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            // Log history entry for task creation.
            await LogTaskHistory(task.Id, $"Task {task.Title} was created");

            return RedirectToAction("Details", "Managers", new { id = task.ManagerId });
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            return View(task);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SrgDomain.Model.Task task)
        {
            if (id != task.Id) return NotFound();

            task.Manager = _context.Managers.FirstOrDefault(m => m.Id == task.ManagerId);
            _context.Update(task);
            await _context.SaveChangesAsync();

            // Log history entry for task modification.
            await LogTaskHistory(task.Id, $"Task {task.Title} was changed");

            return RedirectToAction("Details", "Managers", new { id = task.ManagerId });
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks
                .Include(t => t.Manager)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();

            return View(task);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                // Log the deletion event before removing the task.
                await LogTaskHistory(task.Id, $"Task \"{task.Title}\" was deleted");

                int managerId = task.ManagerId;
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Managers", new { id = managerId });
            }
            return RedirectToAction("Index", "Managers");
        }


        // POST: Tasks/MarkAsCompleted/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsCompleted(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            if (!string.Equals(task.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                task.Status = "Completed";
                _context.Update(task);
                await _context.SaveChangesAsync();

                // Log history entry for marking task as completed.
                await LogTaskHistory(task.Id, $"Task {task.Title} was marked as Completed");
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
