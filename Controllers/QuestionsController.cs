using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QAWebApp.DTOs;
using QAWebApp.Services.Interfaces;
using System.Security.Claims;

namespace QAWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(IQuestionService questionService, ILogger<QuestionsController> logger)
    {
        _questionService = questionService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim);
        var result = await _questionService.CreateQuestionAsync(dto, userId);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message, questionId = result.Question!.Id });
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateQuestion(int id, [FromBody] QuestionUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim);
        var result = await _questionService.UpdateQuestionAsync(id, dto, userId);

        if (!result.Success)
        {
            if (result.Message.Contains("permission", StringComparison.OrdinalIgnoreCase))
                return Forbid(); // 403

            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = result.Message }); // 404

            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim);
        var result = await _questionService.DeleteQuestionAsync(id, userId);

        if (!result.Success)
        {
            if (result.Message.Contains("permission", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = result.Message });

            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }
}
