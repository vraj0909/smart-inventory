using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Common;
using SmartInventory.Data;
using SmartInventory.Models;

namespace SmartInventory.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly InventoryDbContext _context;

        public CategoriesController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View(new Category { Icon = "bi-tag" });
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Icon")] Category category)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == category.Name))
            {
                ModelState.AddModelError("Name", "A category with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                category.CreatedAt = DateTime.UtcNow;
                _context.Add(category);
                await _context.SaveChangesAsync();

                HttpContext.Session.RecordUserAction($"Created category: '{category.Name}'");
                TempData["SuccessMessage"] = $"Category '{category.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Icon,CreatedAt")] Category category)
        {
            if (id != category.Id) return NotFound();

            if (await _context.Categories.AnyAsync(c => c.Name == category.Name && c.Id != category.Id))
            {
                ModelState.AddModelError("Name", "Another category already exists with this name.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.RecordUserAction($"Updated category: '{category.Name}'");
                    TempData["SuccessMessage"] = $"Category '{category.Name}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CategoryExists(category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                if (category.Products.Any())
                {
                    TempData["ErrorMessage"] = $"Cannot delete category '{category.Name}' because it has {category.Products.Count} associated products. Reassign or delete products first.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                HttpContext.Session.RecordUserAction($"Deleted category: '{category.Name}'");
                TempData["SuccessMessage"] = $"Category '{category.Name}' was deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CategoryExists(int id)
        {
            return await _context.Categories.AnyAsync(e => e.Id == id);
        }
    }
}
