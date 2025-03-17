using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SrgDomain.Model;
using SrgInfrastructure;

namespace SrgInfrastructure.Controllers
{
    public class TaskHistoriesController : Controller
    {
        private readonly SrgDatabaseContext _context;

        public TaskHistoriesController(SrgDatabaseContext context)
        {
            _context = context;
        }

        // GET: TaskHistories
        public async Task<IActionResult> Index()
        {
            var srgDatabaseContext = _context.TaskHistories.Include(t => t.Task);
            return View(await srgDatabaseContext.ToListAsync());
        }

        // GET: TaskHistories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var history = await _context.TaskHistories
                .Include(th => th.Task)
                .FirstOrDefaultAsync(th => th.Id == id);
            if (history == null) return NotFound();
            return View(history);
        }

        // GET: TaskHistories/Create?taskId=5
        public IActionResult Create(int taskId)
        {
            ViewBag.TaskId = taskId;
            return View();
        }

        // POST: TaskHistories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskHistory taskHistory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskHistory);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tasks", new { id = taskHistory.TaskId });
            }
            return View(taskHistory);
        }

        // GET: TaskHistories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var history = await _context.TaskHistories.FindAsync(id);
            if (history == null) return NotFound();
            return View(history);
        }

        // POST: TaskHistories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskHistory taskHistory)
        {
            if (id != taskHistory.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskHistory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TaskHistories.Any(e => e.Id == taskHistory.Id))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction("Details", "Tasks", new { id = taskHistory.TaskId });
            }
            return View(taskHistory);
        }

        // GET: TaskHistories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var history = await _context.TaskHistories
                .Include(th => th.Task)
                .FirstOrDefaultAsync(th => th.Id == id);
            if (history == null) return NotFound();
            return View(history);
        }

        // POST: TaskHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var history = await _context.TaskHistories.FindAsync(id);
            if (history != null)
            {
                int? taskId = history.TaskId;
                _context.TaskHistories.Remove(history);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tasks", new { id = taskId });
            }
            return RedirectToAction("Index");
        }

        private bool TaskHistoryExists(int id)
        {
            return _context.TaskHistories.Any(e => e.Id == id);
        }
    }
}
