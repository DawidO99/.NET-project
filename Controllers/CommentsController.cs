using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;

namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize] // Każdy zalogowany może komentować
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Comment comment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            comment.AuthorId = userId;

            // Zmieniamy nazwę klucza obcego zgodnie z modelem `Comment.cs`
            ModelState.Remove("Order");
            ModelState.Remove("Author");

            if (ModelState.IsValid)
            {
                await _commentService.CreateCommentAsync(comment);
            }

            return RedirectToAction("Details", "ServiceOrders", new { id = comment.OrderId });
        }
    }
}