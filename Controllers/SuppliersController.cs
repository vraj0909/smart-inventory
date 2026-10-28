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
    public class SuppliersController : Controller
    {
        private readonly InventoryDbContext _context;

        public SuppliersController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers
                .Include(s => s.Products)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(suppliers);
        }

        // GET: Suppliers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _context.Suppliers
                .Include(s => s.Products)
                    .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // GET: Suppliers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,ContactPerson,Email,Phone,Address,City")] Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                supplier.CreatedAt = DateTime.UtcNow;
                _context.Add(supplier);
                await _context.SaveChangesAsync();

                HttpContext.Session.RecordUserAction($"Registered new supplier: '{supplier.Name}'");
                TempData["SuccessMessage"] = $"Supplier '{supplier.Name}' registered successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ContactPerson,Email,Phone,Address,City,CreatedAt")] Supplier supplier)
        {
            if (id != supplier.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(supplier);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.RecordUserAction($"Updated supplier profile: '{supplier.Name}'");
                    TempData["SuccessMessage"] = $"Supplier '{supplier.Name}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await SupplierExists(supplier.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // GET: Suppliers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _context.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier != null)
            {
                if (supplier.Products.Any())
                {
                    TempData["ErrorMessage"] = $"Cannot delete supplier '{supplier.Name}' because {supplier.Products.Count} products are linked to them. Reassign supplier on those products first.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();

                HttpContext.Session.RecordUserAction($"Deleted supplier: '{supplier.Name}'");
                TempData["SuccessMessage"] = $"Supplier '{supplier.Name}' was deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> SupplierExists(int id)
        {
            return await _context.Suppliers.AnyAsync(e => e.Id == id);
        }
    }
}
