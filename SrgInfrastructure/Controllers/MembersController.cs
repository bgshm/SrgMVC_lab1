using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SrgDomain.Model;
using System.Security.Claims;

namespace SrgInfrastructure.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private readonly SrgDatabaseContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MembersController(SrgDatabaseContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Members – filter for non-admin users.
        [Authorize(Roles = "Member,Manager,Admin")]
        public async Task<IActionResult> Index()
        {
            IQueryable<Member> query = _context.Members.Include(m => m.Manager);
            if (User.IsInRole("Member") || User.IsInRole("Manager"))
            {
                var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user != null && user.DepartmentId.HasValue)
                {
                    query = query.Where(m => m.Manager.DepartmentId == user.DepartmentId.Value);
                }
                else
                {
                    query = query.Where(m => false);
                }
            }
            return View(await query.ToListAsync());
        }

        // GET: Members/Tasks/5 – all roles can view tasks for a member.
        [Authorize(Roles = "Member,Manager,Admin")]
        public async Task<IActionResult> Tasks(int? id)
        {
            if (id == null)
                return NotFound();

            var member = await _context.Members
                .Include(m => m.Manager)                // load the member's own manager
                .Include(m => m.Tasks)                  // load assigned tasks
                    .ThenInclude(t => t.Manager)        // load each task's manager
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
                return NotFound();

            return View(member);
        }


        // GET: Members/Details/5 – filter similarly.
        [Authorize(Roles = "Member,Manager,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            IQueryable<Member> query = _context.Members.Include(m => m.Manager);
            if (User.IsInRole("Member") || User.IsInRole("Manager"))
            {
                var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user != null && user.DepartmentId.HasValue)
                {
                    query = query.Where(m => m.Manager.DepartmentId == user.DepartmentId.Value);
                }
                else
                {
                    return NotFound();
                }
            }

            var member = await query.FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
                return NotFound();

            return View(member);
        }

        // GET: Members/Create?managerId=5 – only Managers and Admins.
        [Authorize(Roles = "Manager,Admin")]
        public IActionResult Create(int managerId)
        {
            ViewBag.ManagerId = managerId;
            return View();
        }

        // POST: Members/Create – only Managers and Admins.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Create(Member member, IFormFile? PhotoFile)
        {
            member.Manager = _context.Managers.FirstOrDefault(m => m.Id == member.ManagerId);

            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                var uploadsFolder = @"D:\Bogdan\Шева\ІСТТП\Второй трай\Lab1_Shumeiko\img\members";
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
                member.PhotoPath = $"/img/members/{uniqueFileName}";
            }

            _context.Add(member);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Managers", new { id = member.ManagerId });
        }

        // GET: Members/Edit/5 – only Managers and Admins.
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound();

            return View(member);
        }

        // POST: Members/Edit/5 – only Managers and Admins.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Edit(int id, Member member, IFormFile? PhotoFile)
        {
            if (id != member.Id)
                return NotFound();

            // 1) Load existing member
            var memToUpdate = await _context.Members.FindAsync(id);
            if (memToUpdate == null)
                return NotFound();

            // 2) Update scalar fields
            memToUpdate.Name = member.Name;
            memToUpdate.Role = member.Role;
            memToUpdate.ManagerId = member.ManagerId;
            memToUpdate.TasksPerMonth = member.TasksPerMonth;
            memToUpdate.TasksTotal = member.TasksTotal;
            memToUpdate.LastTaskDate = member.LastTaskDate;

            // 3) Handle photo upload
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                var uploadsFolder = @"D:\Bogdan\Шева\ІСТТП\Второй трай\Lab1_Shumeiko\img\members";
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(PhotoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await PhotoFile.CopyToAsync(stream);

                memToUpdate.PhotoPath = $"/img/members/{uniqueFileName}";
            }
            // else keep existing memToUpdate.PhotoPath

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Managers", new { id = memToUpdate.ManagerId });
        }


        // GET: Members/Delete/5 – only Managers and Admins.
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Members
                .Include(m => m.Manager)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null) return NotFound();

            return View(member);
        }

        // POST: Members/Delete/5 – only Managers and Admins.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                int managerId = member.ManagerId;
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Managers", new { id = managerId });
            }
            return RedirectToAction("Index", "Managers");
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.Id == id);
        }
    }
}
