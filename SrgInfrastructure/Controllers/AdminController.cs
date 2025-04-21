using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SrgInfrastructure.ViewModels;
using SrgDomain.Model;
using Xceed.Words.NET;
using Xceed.Document.NET;

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

        // GET: Admin/DownloadProtocol
        public async Task<IActionResult> DownloadBonusReport()
        {
            // 1) Prepare date/placeholders
            var now = DateTime.Now;
            var year = now.Year.ToString();
            var month = now.Month.ToString("D2");
            var day = now.Day.ToString("D2");
            var tnr14 = new Formatting
            {
                FontFamily = new Font("Times New Roman"),
                Size = 14D
            };

            // 2) Load the template
            var templatePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Templates",
                "ProtocolTemplate.docx");  // your Протокол №1.docx renamed to ProtocolTemplate.docx
            using var doc = DocX.Load(templatePath);

            // 3) Replace header placeholders
            doc.ReplaceText("YYYY", year);
            doc.ReplaceText("MM", month);
            doc.ReplaceText("DD", day);

            // 4) Fetch data
            var managers = await _context.Managers
                .Include(m => m.Department)
                .ToListAsync();
            var members = await _context.Members
                .Include(m => m.Manager)
                    .ThenInclude(mgr => mgr.Department)
                .ToListAsync();

            // total participants
            int totalParticipants = managers.Count + members.Count;
            doc.ReplaceText("XX", totalParticipants.ToString());

            // compute max tasks among members for scoring
            int maxTasks = members.Max(m => m.TasksTotal ?? 0);

            // 5) Build a combined list of persons
            var persons = new List<(string Name, string Department)>();
            foreach (var mgr in managers)
                persons.Add((mgr.Name, mgr.Department?.DepartmentName ?? "—"));
            foreach (var mem in members)
                persons.Add((mem.Name, mem.Manager?.Department?.DepartmentName ?? "—"));

            // 6) Fill tables
            var tables = doc.Tables;
            if (tables.Count < 8)
                throw new InvalidOperationException("Template must contain 8 tables.");

            // Table 1: Registration (Зареєстрован(-ий/-а))
            var t1 = tables[0];
            t1.RemoveRow(1);
            foreach (var p in persons)
            {
                var row = t1.InsertRow();
                row.Cells[0].Paragraphs[0].Append(p.Name, tnr14);
                row.Cells[1].Paragraphs[0].Append(p.Department, tnr14);
                row.Cells[2].Paragraphs[0].Append("Зареєстрован(-ий/-а)", tnr14);
            }

            // Tables 2–7: Voting results (“За”)
            for (int i = 1; i <= 6; i++)
            {
                var t = tables[i];
                t.RemoveRow(1);
                foreach (var p in persons)
                {
                    var row = t.InsertRow();
                    row.Cells[0].Paragraphs[0].Append(p.Name, tnr14);
                    row.Cells[1].Paragraphs[0].Append("За", tnr14);
                }
            }

            // Table 8: Final summary
            var t8 = tables[7];
            t8.RemoveRow(1);

            // 8a) Managers first
            foreach (var mgr in managers)
            {
                var row = t8.InsertRow();
                row.Cells[0].Paragraphs[0].Append(mgr.Name, tnr14);
                row.Cells[1].Paragraphs[0].Append(mgr.Course.ToString(), tnr14);
                row.Cells[2].Paragraphs[0].Append(mgr.StructuralUnit, tnr14);
                row.Cells[3].Paragraphs[0].Append("2.5", tnr14);
            }

            // 8b) Then members, with scaled points (max → 2.5)
            foreach (var mem in members)
            {
                double pts = maxTasks > 0
                    ? Math.Round((mem.TasksTotal ?? 0) / (double)maxTasks * 2, 1)
                    : 0.0;

                var row = t8.InsertRow();
                row.Cells[0].Paragraphs[0].Append(mem.Name, tnr14);
                row.Cells[1].Paragraphs[0].Append(mem.Course.ToString(), tnr14);
                //row.Cells[1].Paragraphs[0].Append(mem.Manager?.Department?.DepartmentName ?? "—", tnr14);
                //row.Cells[2].Paragraphs[0].Append((mem.TasksTotal ?? 0).ToString(), tnr14);
                row.Cells[2].Paragraphs[0].Append(mem.StructuralUnit, tnr14);
                row.Cells[3].Paragraphs[0].Append(pts.ToString("F1"), tnr14);
            }

            // 7) Save and return
            var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports");
            Directory.CreateDirectory(reportsDir);
            var fileName = $"Protocol_{now:yyyyMMdd}.docx";
            var outputPath = Path.Combine(reportsDir, fileName);
            doc.SaveAs(outputPath);

            return PhysicalFile(
                outputPath,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName);
        }

    }
}
