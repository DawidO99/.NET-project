using CarWorkshopManagementSystem.Models;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Services
{
    public interface ICommentService
    {
        Task CreateCommentAsync(Comment comment);
    }
}