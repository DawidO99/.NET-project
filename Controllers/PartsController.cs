// Controllers/PartsController.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Dodaj to using dla DbUpdateConcurrencyException
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Controllers
{
    // Autoryzacja: tylko Admin i Recepcjonista mogą zarządzać częściami
    [Authorize(Roles = "Admin,Recepcjonista")]
    public class PartsController : Controller
    {
        private readonly IPartService _partService;

        public PartsController(IPartService partService)
        {
            _partService = partService;
        }

        // GET: Parts
        public async Task<IActionResult> Index()
        {
            var parts = await _partService.GetAllPartsAsync();
            return View(parts);
        }

        // GET: Parts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var part = await _partService.GetPartByIdAsync(id.Value);
            if (part == null)
            {
                return NotFound();
            }
            return View(part);
        }

        // GET: Parts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Parts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Type,UnitPrice")] Part part)
        {
            if (ModelState.IsValid)
            {
                await _partService.CreatePartAsync(part);
                return RedirectToAction(nameof(Index));
            }
            return View(part);
        }

        // GET: Parts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var part = await _partService.GetPartByIdAsync(id.Value);
            if (part == null)
            {
                return NotFound();
            }
            return View(part);
        }

        // POST: Parts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,UnitPrice")] Part part)
        {
            if (id != part.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _partService.UpdatePartAsync(part);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Dodaj logikę obsługi konfliktu współbieżności, jeśli będzie potrzebna
                    // np. sprawdzenie, czy element nadal istnieje
                    if (await _partService.GetPartByIdAsync(part.Id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(part);
        }

        // GET: Parts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var part = await _partService.GetPartByIdAsync(id.Value);
            if (part == null)
            {
                return NotFound();
            }

            return View(part);
        }

        // POST: Parts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _partService.DeletePartAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}