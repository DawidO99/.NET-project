// Controllers/CommentsController.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly UserManager<AppUser> _userManager;

        public CommentsController(ICommentService commentService, UserManager<AppUser> userManager)
        {
            _commentService = commentService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // --- GŁÓWNA ZMIANA TUTAJ ---
        // Zamiast bindować cały obiekt Comment, przyjmujemy proste parametry z formularza.
        // Nazwy parametrów (orderId, content) muszą pasować do atrybutów 'name' w formularzu.
        // W Twoim formularzu jest <input name="OrderId"> i <textarea name="Content">, więc wielkość liter ma znaczenie.
        // Aby to ujednolicić, użyjemy atrybutu [FromForm].
        public async Task<IActionResult> Create([FromForm(Name = "OrderId")] int orderId, [FromForm(Name = "Content")] string content)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Ręcznie tworzymy kompletny obiekt Comment
            var comment = new Comment
            {
                OrderId = orderId,
                Content = content,
                AuthorId = currentUser.Id
            };

            // Teraz, gdy obiekt jest kompletny, możemy go bezpiecznie zwalidować.
            if (TryValidateModel(comment))
            {
                // Ścieżka sukcesu
                await _commentService.CreateCommentAsync(comment);
                TempData["SuccessMessage"] = "Komentarz został pomyślnie dodany.";
                return RedirectToAction("Details", "ServiceOrders", new { id = comment.OrderId });
            }
            else
            {
                // Ścieżka błędu (np. pusta treść)
                var errorMessages = ModelState.Values
                                         .SelectMany(v => v.Errors)
                                         .Select(e => e.ErrorMessage)
                                         .ToList();

                TempData["CommentValidationError"] = string.Join(" ", errorMessages);
                return RedirectToAction("Details", "ServiceOrders", new { id = orderId });
            }
        }
    }
}