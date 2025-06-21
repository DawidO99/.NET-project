// Controllers/CommentsController.cs
using CarWorkshopManagementSystem.Models;
using CarWorkshopManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Controllers
{
    [Authorize] // Autoryzacja dla komentarzy, np. każdy zalogowany użytkownik
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
        public async Task<IActionResult> Create([Bind("Content,OrderId")] Comment comment)
        {
            // Usunięcie walidacji dla właściwości nawigacyjnych
            ModelState.Remove("Author");
            ModelState.Remove("Order");

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    // Użytkownik niezalogowany lub błąd. Możesz przekierować na stronę logowania lub zwrócić błąd.
                    return Unauthorized();
                }

                comment.AuthorId = currentUser.Id;
                // CreatedAt jest ustawiane w CommentService

                await _commentService.CreateCommentAsync(comment);
                // Przekieruj z powrotem na stronę szczegółów zlecenia, z którego przyszedł komentarz
                return RedirectToAction("Details", "ServiceOrders", new { id = comment.OrderId });
            }

            // Jeśli model nie jest poprawny, można zwrócić widok z błędem walidacji,
            // ale dla komentarza na stronie szczegółów zlecenia, lepsze jest przekierowanie
            // z informacją o błędzie. TempData może być użyte.
            TempData["ErrorMessage"] = "Wystąpił błąd podczas dodawania komentarza. Upewnij się, że pole nie jest puste.";
            return RedirectToAction("Details", "ServiceOrders", new { id = comment.OrderId });
        }
    }
}