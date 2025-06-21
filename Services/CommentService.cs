using CarWorkshopManagementSystem.Data;
using CarWorkshopManagementSystem.Models;
using System;
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateCommentAsync(Comment comment)
        {
            comment.CreatedAt = DateTime.UtcNow;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }
    }
}