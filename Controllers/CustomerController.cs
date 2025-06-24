// Controllers/CustomerController.cs
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using CarWorkshopManagementSystem.DTOs;
using CarWorkshopManagementSystem.Mappers;

namespace CarWorkshopManagementSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly CustomerMapper _customerMapper;

        public CustomerController(ICustomerService customerService, CustomerMapper customerMapper)
        {
            _customerService = customerService;
            _customerMapper = customerMapper;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllAsync();
            var customerDtos = _customerMapper.Map(customers);
            return View(customerDtos);
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            var customerDto = _customerMapper.Map(customer);
            return View(customerDto);
        }

        public IActionResult Create()
        {
            return View(new CustomerCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerCreateDto customerCreateDto)
        {
            if (ModelState.IsValid)
            {
                var customer = _customerMapper.Map(customerCreateDto);
                await _customerService.AddAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            return View(customerCreateDto);
        }

        // GET: /Customer/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            // ZMIENIONO: Używamy MapToUpdateDto
            var customerUpdateDto = _customerMapper.MapToUpdateDto(customer); // Użycie nowej metody mapującej
            return View(customerUpdateDto);
        }

        // POST: /Customer/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        // UWAGA: Teraz customerUpdateDto ma Id, więc można sprawdzić id != customerUpdateDto.Id
        public async Task<IActionResult> Edit(int id, CustomerUpdateDto customerUpdateDto)
        {
            if (id != customerUpdateDto.Id) // Walidacja ID z URL vs ID z DTO
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _customerService.GetByIdAsync(id);
                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }

                    _customerMapper.Map(customerUpdateDto, existingCustomer);

                    await _customerService.UpdateAsync(existingCustomer);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _customerService.GetByIdAsync(id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index)); // Przekierowanie do Index
            }
            return View(customerUpdateDto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _customerService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}