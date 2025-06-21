// Controllers/ServiceOrdersController.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using CarWorkshopManagementSystem.Extensions;
using Microsoft.EntityFrameworkCore;
using CarWorkshopManagementSystem.Data; // Dodaj tę linię

namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Recepcjonista,Mechanik")]
    public class ServiceOrdersController : Controller
    {
        private readonly IServiceOrderService _orderService;
        private readonly IVehicleService _vehicleService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IPartService _partService;
        private readonly ApplicationDbContext _context;

        public ServiceOrdersController(IServiceOrderService orderService, IVehicleService vehicleService, UserManager<AppUser> userManager, IPartService partService, ApplicationDbContext context)
        {
            _orderService = orderService;
            _vehicleService = vehicleService;
            _userManager = userManager;
            _partService = partService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VehicleId,Description,AssignedMechanicId")] ServiceOrder serviceOrder)
        {
            ModelState.Remove("Vehicle");

            if (ModelState.IsValid)
            {
                await _orderService.CreateOrderAsync(serviceOrder);
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(serviceOrder);
            return View(serviceOrder);
        }

        // GET: ServiceOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceOrder = await _orderService.GetOrderByIdAsync(id.Value);

            if (serviceOrder == null)
            {
                return NotFound();
            }

            var parts = await _partService.GetAllPartsAsync();
            ViewBag.Parts = parts.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Name} ({p.UnitPrice:C})"
            }).ToList();


            return View(serviceOrder);
        }

        // NOWE AKCJE DLA ZMIANY STATUSU ZLECENIA (US9)

        // GET: ServiceOrders/ChangeStatus/5
        [Authorize(Roles = "Admin,Mechanik")]
        public async Task<IActionResult> ChangeStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceOrder = await _orderService.GetOrderByIdAsync(id.Value);
            if (serviceOrder == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || (!User.IsInRole("Admin") && serviceOrder.AssignedMechanicId != currentUser.Id))
            {
                return Forbid();
            }

            ViewBag.CurrentStatus = serviceOrder.Status;

            ViewBag.Statuses = Enum.GetValues(typeof(ServiceOrderStatus))
                                           .Cast<ServiceOrderStatus>()
                                           .Select(e => new SelectListItem
                                           {
                                               Value = e.ToString(),
                                               Text = e.GetDisplayName(),
                                               Selected = (e == serviceOrder.Status)
                                           }).ToList();

            return View(serviceOrder);
        }

        // POST: ServiceOrders/ChangeStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Mechanik")]
        public async Task<IActionResult> ChangeStatus(int id, ServiceOrderStatus Status)
        {
            var serviceOrder = await _orderService.GetOrderByIdAsync(id);
            if (serviceOrder == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || (!User.IsInRole("Admin") && serviceOrder.AssignedMechanicId != currentUser.Id))
            {
                return Forbid();
            }

            serviceOrder.Status = Status;

            try
            {
                await _orderService.UpdateOrderAsync(serviceOrder);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Wystąpił błąd podczas aktualizacji statusu zlecenia.");
                ViewBag.CurrentStatus = serviceOrder.Status;
                ViewBag.Statuses = Enum.GetValues(typeof(ServiceOrderStatus))
                                           .Cast<ServiceOrderStatus>()
                                           .Select(e => new SelectListItem
                                           {
                                               Value = e.ToString(),
                                               Text = e.GetDisplayName(),
                                               Selected = (e == serviceOrder.Status)
                                           }).ToList();
                return View(serviceOrder);
            }

            return RedirectToAction(nameof(Details), new { id = serviceOrder.Id });
        }

        // AKCJE DLA ZADAŃ (ServiceTask) - DODANE TERAZ!
        // POST: ServiceOrders/AddTask/{id} (id to ServiceOrderId)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Mechanik")]
        public async Task<IActionResult> AddTask(int id, [Bind("Description,LaborCost,Status")] ServiceTask newTask, List<int>? selectedPartIds, List<int>? quantities)
        {
            if (ModelState.IsValid)
            {
                newTask.ServiceOrderId = id;

                if (selectedPartIds != null && quantities != null)
                {
                    for (int i = 0; i < selectedPartIds.Count; i++)
                    {
                        if (selectedPartIds[i] > 0 && quantities[i] > 0)
                        {
                            newTask.UsedParts.Add(new UsedPart
                            {
                                PartId = selectedPartIds[i],
                                Quantity = quantities[i],
                                ServiceTaskId = newTask.Id
                            });
                        }
                    }
                }

                _context.ServiceTasks.Add(newTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var serviceOrder = await _orderService.GetOrderByIdAsync(id);
            ViewBag.Parts = new SelectList(await _partService.GetAllPartsAsync(), "Id", "Name", "UnitPrice");
            return View("Details", serviceOrder);
        }


        // GET: ServiceOrders/EditTask/{id} (id to ServiceTaskId)
        [Authorize(Roles = "Admin,Mechanik")]
        public async Task<IActionResult> EditTask(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                    .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            ViewBag.Parts = new SelectList(await _partService.GetAllPartsAsync(), "Id", "Name", "UnitPrice");
            ViewBag.ServiceOrderId = task.ServiceOrderId;

            return View(task);
        }

        // POST: ServiceOrders/EditTask/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Mechanik")]
        public async Task<IActionResult> EditTask(int id, [Bind("Id,Description,LaborCost,Status,ServiceOrderId")] ServiceTask task, List<int>? selectedPartIds, List<int>? quantities)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTask = await _context.ServiceTasks
                        .Include(t => t.UsedParts)
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (existingTask == null)
                    {
                        return NotFound();
                    }

                    existingTask.Description = task.Description;
                    existingTask.LaborCost = task.LaborCost;
                    existingTask.Status = task.Status;

                    _context.UsedParts.RemoveRange(existingTask.UsedParts);
                    existingTask.UsedParts.Clear();

                    if (selectedPartIds != null && quantities != null)
                    {
                        for (int i = 0; i < selectedPartIds.Count; i++)
                        {
                            if (selectedPartIds[i] > 0 && quantities[i] > 0)
                            {
                                existingTask.UsedParts.Add(new UsedPart
                                {
                                    PartId = selectedPartIds[i],
                                    Quantity = quantities[i],
                                    ServiceTaskId = existingTask.Id
                                });
                            }
                        }
                    }

                    _context.Update(existingTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ServiceTasks.Any(e => e.Id == task.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = task.ServiceOrderId });
            }

            ViewBag.Parts = new SelectList(await _partService.GetAllPartsAsync(), "Id", "Name", "UnitPrice");
            ViewBag.ServiceOrderId = task.ServiceOrderId;
            return View(task);
        }

        // GET: ServiceOrders/DeleteTask/{id} (id to ServiceTaskId)
        [Authorize(Roles = "Admin,Mechanik")]
        public async Task<IActionResult> DeleteTask(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.ServiceTasks
                .Include(t => t.ServiceOrder)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        // POST: ServiceOrders/DeleteTask/{id}
        [HttpPost, ActionName("DeleteTask")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Mechanik")]
        public async Task<IActionResult> DeleteTaskConfirmed(int id)
        {
            var task = await _context.ServiceTasks
                .Include(t => t.UsedParts)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            int serviceOrderId = task.ServiceOrderId;

            _context.ServiceTasks.Remove(task);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = serviceOrderId });
        }


        // Prywatna metoda pomocnicza do ładowania danych dla dropdownów
        private async Task PopulateDropdowns(ServiceOrder? model = null)
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            var mechanics = await _userManager.GetUsersInRoleAsync("Mechanik");

            var vehicleList = vehicles.Select(v => new
            {
                v.Id,
                DisplayText = $"{v.Brand} {v.Model} ({v.RegistrationNumber})"
            });

            ViewBag.Vehicles = new SelectList(vehicleList, "Id", "DisplayText", model?.VehicleId);
            ViewBag.Mechanics = new SelectList(mechanics, "Id", "UserName", model?.AssignedMechanicId);
        }
    }
}