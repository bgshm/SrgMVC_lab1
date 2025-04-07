using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SrgDomain.Model;
using System.Security.Claims;

namespace SrgInfrastructure.Controllers
{
    [Authorize] // All actions require an authenticated user.
    public class DepartmentsController : Controller
    {
        private readonly SrgDatabaseContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DepartmentsController(SrgDatabaseContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Departments – non-admins see only their department.
        [Authorize(Roles = "Member,Manager,Admin")]
        public async Task<IActionResult> Index()
        {
            IQueryable<Department> query = _context.Departments.Include(d => d.Manager);

            if (User.IsInRole("Member") || User.IsInRole("Manager"))
            {
                var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user != null && user.DepartmentId.HasValue)
                {
                    query = query.Where(d => d.Id == user.DepartmentId.Value);
                }
                else
                {
                    // Optionally return an empty list if the user isn’t associated with a department.
                    query = query.Where(d => false);
                }
            }

            var departments = await query.ToListAsync();
            return View(departments);
        }

        // GET: Departments/Details/5 – same filtering.
        [Authorize(Roles = "Member,Manager,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            IQueryable<Department> query = _context.Departments.Include(d => d.Manager);

            if (User.IsInRole("Member") || User.IsInRole("Manager"))
            {
                var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user != null && user.DepartmentId.HasValue)
                {
                    query = query.Where(d => d.Id == user.DepartmentId.Value);
                }
                else
                {
                    return NotFound();
                }
            }

            var department = await query.FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
                return NotFound();

            return View(department);
        }

        // GET: Departments/Create – only Managers and Admins.
        [Authorize(Roles = "Manager,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create – only Managers and Admins.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Create(Department department, IFormFile? PhotoFile)
        {
            if (ModelState.IsValid)
            {
                // If a file is uploaded, process and save it.
                if (PhotoFile != null && PhotoFile.Length > 0)
                {
                    var uploadsFolder = @"D:\Bogdan\Шева\ІСТТП\Второй трай\Lab1_Shumeiko\img\departments";
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
                    // Save the relative path to the database.
                    department.PhotoPath = $"/img/departments/{uniqueFileName}";
                }

                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Departments/Edit/5 – only Managers and Admins.
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            return View(department);
        }

        // POST: Departments/Edit/5 – only Managers and Admins.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Edit(int id, Department department, IFormFile? PhotoFile)
        {
            if (id != department.Id)
                return NotFound();

            // 1) Load the existing department
            var deptToUpdate = await _context.Departments.FindAsync(id);
            if (deptToUpdate == null)
                return NotFound();

            // 2) Update scalar fields
            deptToUpdate.DepartmentName = department.DepartmentName;
            deptToUpdate.Description = department.Description;
            deptToUpdate.ContactEmail = department.ContactEmail;
            // Note: CreatedDate is not changed

            // 3) Handle photo upload if provided
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                var uploadsFolder = @"D:\Bogdan\Шева\ІСТТП\Второй трай\Lab1_Shumeiko\img\departments";
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(PhotoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await PhotoFile.CopyToAsync(stream);

                // Update the PhotoPath
                deptToUpdate.PhotoPath = $"/img/departments/{uniqueFileName}";
            }
            // else: no PhotoFile => keep existing deptToUpdate.PhotoPath

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Departments.Any(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Departments/Delete/5 – only Managers and Admins.
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Departments.FirstOrDefaultAsync(m => m.Id == id);
            if (department == null) return NotFound();

            return View(department);
        }

        // POST: Departments/Delete/5 – only Managers and Admins.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }
    }
}
