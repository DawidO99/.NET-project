using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Controllers
{
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
            return View(await _partService.GetAllPartsAsync());
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
            if (id == null) return NotFound();
            var part = await _partService.GetPartByIdAsync(id.Value);
            if (part == null) return NotFound();
            return View(part);
        }

        // POST: Parts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,UnitPrice")] Part part)
        {
            if (id != part.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _partService.UpdatePartAsync(part);
                return RedirectToAction(nameof(Index));
            }
            return View(part);
        }

        // Inne akcje jak Delete dodasz w ten sam sposób
    }
}