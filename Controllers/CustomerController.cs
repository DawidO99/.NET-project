using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic; // To już masz
using CarWorkshopManagementSystem.Models; // To już masz
using CarWorkshopManagementSystem.Services; // To już masz

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

        // GET: /Customer/Details/{id}
        public async Task<IActionResult> Details(int id) // <--- DODANA AKCJA DETAILS
        {
            var customer = await _service.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound(); // Zwróć 404 Not Found, jeśli klienta o danym ID nie ma
            }
            return View(customer);
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