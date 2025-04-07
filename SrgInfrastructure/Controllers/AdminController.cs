using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SrgInfrastructure.ViewModels;
using SrgDomain.Model;
using Xceed.Words.NET;

namespace SrgInfrastructure.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SrgDatabaseContext _context;

        public AdminController(UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager,
                               SrgDatabaseContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Admin/Index
        public async Task<IActionResult> Index()
        {
            // Get all users except the admin user (assuming admin's email is admin@example.com)
            var users = _userManager.Users.Where(u => u.Email != "admin@example.com").ToList();
            var model = new List<AdminUserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var currentRole = roles.FirstOrDefault() ?? "Member"; // default to Member if no role
                model.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    CurrentRole = currentRole,
                    SelectedRole = currentRole,
                    DepartmentId = user.DepartmentId
                });
            }

            // Pass available departments and roles via ViewBag for dropdowns.
            ViewBag.Departments = await _context.Departments
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.DepartmentName })
                .ToListAsync();
            ViewBag.AvailableRoles = new List<SelectListItem>
            {
                new SelectListItem { Text = "Member", Value = "Member" },
                new SelectListItem { Text = "Manager", Value = "Manager" }
            };

            return View(model);
        }

        // POST: Admin/UpdateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(AdminUserViewModel model)
        {
            // Find user by Id.
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Update role if necessary.
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(model.SelectedRole))
            {
                // Remove all roles (if you assume a user has only one role) and add the new role.
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }

            // Update DepartmentId.
            user.DepartmentId = model.DepartmentId;
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/DownloadBonusReport
        public async Task<IActionResult> DownloadBonusReport()
        {
            // 1) Load the Word template
            var templatePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Templates",
                "ReportTemplate.docx");
            using var doc = DocX.Load(templatePath);


            // 2) Get the first table and remove its placeholder row
            var table = doc.Tables.First();
            table.RemoveRow(1);

            // 3) Fetch members and compute maximum tasks
            var members = await _context.Members
                .Include(m => m.Manager)
                .ThenInclude(mgr => mgr.Department)
                .ToListAsync();
            int maxTasks = members.Max(m => m.TasksTotal ?? 0);

            // 4) Fill the table
            foreach (var m in members)
            {
                double points = maxTasks > 0
                    ? Math.Round((m.TasksTotal ?? 0) / (double)maxTasks * 2.0, 1)
                    : 0.0;

                var row = table.InsertRow();
                row.Cells[0].Paragraphs[0].Append(m.Name);
                row.Cells[1].Paragraphs[0].Append(m.Manager?.Department?.DepartmentName ?? "—");
                row.Cells[2].Paragraphs[0].Append((m.TasksTotal ?? 0).ToString());
                row.Cells[3].Paragraphs[0].Append(points.ToString("F1"));
            }

            // 5) Save the filled document
            var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "reports");
            Directory.CreateDirectory(reportsDir);
            var fileName = $"BonusReport_{DateTime.Now:yyyyMMdd_HHmm}.docx";
            var outputPath = Path.Combine(reportsDir, fileName);
            doc.SaveAs(outputPath);

            // 6) Return the file to the browser
            return PhysicalFile(
                outputPath,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName);
        }
    }
}
