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
            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SrgDomain.Model.Task task)
        {
            task.Manager = _context.Managers.FirstOrDefault(m => m.Id == task.ManagerId);

            _context.Add(task);
            await _context.SaveChangesAsync();
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
                int managerId = task.ManagerId;
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Managers", new { id = managerId });
            }
            return RedirectToAction("Index", "Managers");
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
