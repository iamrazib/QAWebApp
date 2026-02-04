using Microsoft.EntityFrameworkCore;
using QAWebApp.Data;
using QAWebApp.DTOs;
using QAWebApp.Models;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Services.Implementations;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CommentService> _logger;

    public CommentService(ApplicationDbContext context, ILogger<CommentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, Comment? Comment)> CreateCommentAsync(CommentCreateDto dto, int userId)
    {
        try
        {
            if (dto.QuestionId == null && dto.AnswerId == null)
            {
                return (false, "Comment must be associated with a question or answer", null);
            }

            var comment = new Comment
            {
                Body = dto.Body,
                UserId = userId,
                QuestionId = dto.QuestionId,
                AnswerId = dto.AnswerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Comment {CommentId} created by user {UserId}", comment.Id, userId);
            return (true, "Comment posted successfully", comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment");
            return (false, "An error occurred while posting the comment", null);
        }
    }

    public async Task<List<Comment>> GetCommentsByQuestionIdAsync(int questionId)
    {
        try
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.QuestionId == questionId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for question {QuestionId}", questionId);
            return new List<Comment>();
        }
    }

    public async Task<List<Comment>> GetCommentsByAnswerIdAsync(int answerId)
    {
        try
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.AnswerId == answerId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for answer {AnswerId}", answerId);
            return new List<Comment>();
        }
    }


    public async Task<(bool Success, string Message)> UpdateCommentAsync(int commentId, CommentUpdateDto dto, int userId)
    {
        try
        {
            var c = await _context.Comments.FindAsync(commentId);
            if (c == null) return (false, "Comment not found");

            if (c.UserId != userId) return (false, "You do not have permission to update this comment");

            c.Body = dto.Body;
            await _context.SaveChangesAsync();
            return (true, "Comment updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment {CommentId}", commentId);
            return (false, "An error occurred while updating the comment");
        }
    }

    public async Task<(bool Success, string Message)> DeleteCommentAsync(int commentId, int userId)
    {
        try
        {
            var c = await _context.Comments.FindAsync(commentId);
            if (c == null) return (false, "Comment not found");

            if (c.UserId != userId) return (false, "You do not have permission to delete this comment");

            _context.Comments.Remove(c);
            await _context.SaveChangesAsync();
            return (true, "Comment deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return (false, "An error occurred while deleting the comment");
        }
    }
}
