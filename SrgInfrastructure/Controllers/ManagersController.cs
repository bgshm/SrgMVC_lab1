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
    public class ManagersController : Controller
    {
        private readonly SrgDatabaseContext _context;

        public ManagersController(SrgDatabaseContext context)
        {
            _context = context;
        }

        // GET: Managers
        public async Task<IActionResult> Index()
        {
            var srgDatabaseContext = _context.Managers.Include(m => m.Department);
            return View(await srgDatabaseContext.ToListAsync());
        }

        // GET: Managers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var manager = await _context.Managers
                .Include(m => m.Department)
                .Include(m => m.Members)
                .Include(m => m.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (manager == null) return NotFound();

            return View(manager);
        }

        // GET: Managers/Create?departmentId=5
        public IActionResult Create(int departmentId)
        {
            ViewBag.DepartmentId = departmentId;
            return View();
        }

        // POST: Managers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Manager manager)
        {
            manager.Department = _context.Departments.FirstOrDefault(d => d.Id == manager.DepartmentId);

            _context.Add(manager);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Departments", new { id = manager.DepartmentId });
        }

        // GET: Managers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var manager = await _context.Managers
                .Include(m => m.Department)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (manager == null) return NotFound();

            return View(manager);
        }


        // POST: Managers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Manager manager)
        {
            if (id != manager.Id) return NotFound();

            manager.Department = _context.Departments.FirstOrDefault(d => d.Id == manager.DepartmentId);
            _context.Update(manager);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Departments", new { id = manager.DepartmentId });          
        }

        // GET: Managers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var manager = await _context.Managers
                .Include(m => m.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (manager == null) return NotFound();

            return View(manager);
        }

        // POST: Managers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var manager = await _context.Managers.FindAsync(id);
            if (manager != null)
            {
                int deptId = manager.DepartmentId;
                _context.Managers.Remove(manager);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Departments", new { id = deptId });
            }
            return RedirectToAction("Index", "Departments");
        }

        private bool ManagerExists(int id)
        {
            return _context.Managers.Any(e => e.Id == id);
        }
    }
}
