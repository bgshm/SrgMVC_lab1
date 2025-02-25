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
    public class MembersController : Controller
    {
        private readonly SrgDatabaseContext _context;

        public MembersController(SrgDatabaseContext context)
        {
            _context = context;
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Members
                .Include(m => m.Manager)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null) return NotFound();

            return View(member);
        }

        // GET: Members/Create?managerId=5
        public IActionResult Create(int managerId)
        {
            ViewBag.ManagerId = managerId;
            return View();
        }

        // POST: Members/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Member member)
        {
            member.Manager = _context.Managers.FirstOrDefault(m => m.Id == member.ManagerId);
            member.ManagerId = _context.Managers.FirstOrDefault(m => m.Id == member.ManagerId).Id;

            _context.Add(member);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Managers", new { id = member.ManagerId });         
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound();

            return View(member);
        }

        // POST: Members/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Member member)
        {
            if (id != member.Id) return NotFound();
            member.Manager = _context.Managers.FirstOrDefault(m => m.Id == member.ManagerId);

            _context.Update(member);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Managers", new { id = member.ManagerId });
           
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Members
                .Include(m => m.Manager)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null) return NotFound();

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
