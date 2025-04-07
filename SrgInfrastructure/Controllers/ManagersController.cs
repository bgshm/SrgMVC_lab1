using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SrgDomain.Model;
using System.Security.Claims;

namespace SrgInfrastructure.Controllers
{
    [Authorize]
    public class ManagersController : Controller
    {
        private readonly SrgDatabaseContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManagersController(SrgDatabaseContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Managers – if user is non-admin, filter by department.
        [Authorize(Roles = "Member,Manager,Admin")]
        public async Task<IActionResult> Index()
        {
            IQueryable<Manager> query = _context.Managers.Include(m => m.Department);
            if (User.IsInRole("Member") || User.IsInRole("Manager"))
            {
                var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user != null && user.DepartmentId.HasValue)
                {
                    query = query.Where(m => m.DepartmentId == user.DepartmentId.Value);
                }
                else
                {
                    query = query.Where(m => false);
                }
            }
            return View(await query.ToListAsync());
        }

        // GET: Managers/Details/5 – same filtering.
        [Authorize(Roles = "Member,Manager,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            IQueryable<Manager> query = _context.Managers
                .Include(m => m.Department)
                .Include(m => m.Members)
                .Include(m => m.Tasks);

            if (User.IsInRole("Member") || User.IsInRole("Manager"))
            {
                var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user != null && user.DepartmentId.HasValue)
                {
                    query = query.Where(m => m.DepartmentId == user.DepartmentId.Value);
                }
                else
                {
                    return NotFound();
                }
            }

            var manager = await query.FirstOrDefaultAsync(m => m.Id == id);
            if (manager == null)
                return NotFound();

            return View(manager);
        }

        // GET: Managers/Create?departmentId=5 – only Managers and Admins.
        [Authorize(Roles = "Manager,Admin")]
        public IActionResult Create(int departmentId)
        {
            ViewBag.DepartmentId = departmentId;
            return View();
        }

        // POST: Managers/Create – only Managers and Admins.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Create(Manager manager, IFormFile? PhotoFile)
        {
            // Associate the manager with a department.
            manager.Department = _context.Departments.FirstOrDefault(d => d.Id == manager.DepartmentId);

            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                var uploadsFolder = @"D:\Bogdan\Шева\ІСТТП\Второй трай\Lab1_Shumeiko\img\managers";
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(PhotoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await PhotoFile.CopyToAsync(stream);
                }
                manager.PhotoPath = $"/img/managers/{uniqueFileName}";
            }

            _context.Add(manager);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Departments", new { id = manager.DepartmentId });
        }

        // GET: Managers/Edit/5 – only Managers and Admins.
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var manager = await _context.Managers
                .Include(m => m.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (manager == null) return NotFound();

            return View(manager);
        }

        // POST: Managers/Edit/5 – only Managers and Admins.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Edit(int id, Manager manager, IFormFile? PhotoFile)
        {
            if (id != manager.Id)
                return NotFound();

            // 1) Load existing manager
            var mgrToUpdate = await _context.Managers.FindAsync(id);
            if (mgrToUpdate == null)
                return NotFound();

            // 2) Update scalar fields
            mgrToUpdate.Name = manager.Name;
            mgrToUpdate.DepartmentId = manager.DepartmentId;

            // 3) Handle photo upload
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                var uploadsFolder = @"D:\Bogdan\Шева\ІСТТП\Второй трай\Lab1_Shumeiko\img\managers";
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(PhotoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await PhotoFile.CopyToAsync(stream);

                mgrToUpdate.PhotoPath = $"/img/managers/{uniqueFileName}";
            }
            // else keep existing mgrToUpdate.PhotoPath

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Departments", new { id = mgrToUpdate.DepartmentId });
        }


        // GET: Managers/Delete/5 – only Managers and Admins.
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var manager = await _context.Managers
                .Include(m => m.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (manager == null) return NotFound();

            return View(manager);
        }

        // POST: Managers/Delete/5 – only Managers and Admins.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
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
