// Controllers/CustomerController.cs
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic; // Potrzebne dla List<Customer>
using CarWorkshopManagementSystem.Models; // Upewnij się, że to jest!
using CarWorkshopManagementSystem.Services; // Upewnij się, że to jest!

namespace CarWorkshopManagementSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _service;

        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        // GET: /Customer
        public async Task<IActionResult> Index()
        {
            var customers = await _service.GetAllAsync();
            return View(customers);
        }

        // GET: /Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // Dobra praktyka dla bezpieczeństwa
        public async Task<IActionResult> Create([Bind("FullName,PhoneNumber")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _service.AddAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }
    }
}