using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskFlow.API.Data;
using TaskFlow.API.Models;

namespace TaskFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Tester")]
    public class TesterController : ControllerBase
    {
        private readonly TaskFlowDbContext _context;

        public TesterController(TaskFlowDbContext context)
        {
            _context = context;
        }

        [HttpGet("ready-for-test")]
        public async Task<IActionResult> GetReadyForTestTasks()
        {
            // Angular'ın HTML'de listelemesi için Include ile Proje adını da çekiyoruz
            var readyTasks = await _context.Tasks
                .Include(t => t.Project)
                .Where(t => t.Status == "READY FOR TEST") 
                .Select(t => new {
                    taskId = t.Id, // ID eşleştirmesi
                    description = t.Description,
                    projectName = t.Project != null ? t.Project.ProjectName : "Bilinmeyen Proje"
                })
                .ToListAsync();

            // Alt kısımdaki istatistikler için sayıları hesaplıyoruz
            var totalBugs = await _context.Tasks.CountAsync(t => t.Status == "BUG OLUŞTURULDU");
            var resolvedBugs = await _context.Tasks.CountAsync(t => t.Status == "TAMAMLANDI");

            return Ok(new { tasks = readyTasks, toplamBug = totalBugs, cozulenBug = resolvedBugs });
        }

        [HttpPut("evaluate-task")]
        public async Task<IActionResult> EvaluateTask([FromBody] EvaluateTaskDto dto)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == dto.TaskId);
            if (task == null) return NotFound("Görev bulunamadı.");

            var testerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int parsedTesterId = int.Parse(testerIdString ?? "0");

            if (dto.IsSuccessful)
            {
                task.Status = "TAMAMLANDI"; 
            }
            else
            {
                task.Status = "BUG OLUŞTURULDU"; 
            }

            if (!string.IsNullOrEmpty(dto.CommentText))
            {
                var comment = new Comment 
                {
                    TaskId = dto.TaskId,
                    UserId = parsedTesterId,
                    CommentText = dto.CommentText 
                };
                _context.Comments.Add(comment);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Görev değerlendirmesi tamamlandı. Yeni Durum: {task.Status}" });
        }

        [HttpPost("add-comment")]
        public async Task<IActionResult> AddComment([FromBody] TesterCommentDto dto)
        {
            var testerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int parsedTesterId = int.Parse(testerIdString ?? "0");
            
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == dto.TaskId);
            if (task == null) return NotFound("Görev bulunamadı.");

            var comment = new Comment
            {
                TaskId = dto.TaskId,
                UserId = parsedTesterId,
                CommentText = dto.CommentText ?? "" 
            };
            
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Yorum başarıyla eklendi." });
        }
    }

    public class EvaluateTaskDto
    {
        public int TaskId { get; set; }
        public bool IsSuccessful { get; set; } 
        public string? CommentText { get; set; } 
    }

    public class TesterCommentDto
    {
        public int TaskId { get; set; }
        public string? CommentText { get; set; }
    }
}