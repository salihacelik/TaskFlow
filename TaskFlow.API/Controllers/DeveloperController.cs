using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using TaskFlow.API.Data; 
using TaskFlow.API.Models;

namespace TaskFlow.API.Controllers
{
    [Authorize(Roles = "Developer")]
    [Route("api/[controller]")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        private readonly TaskFlowDbContext _context;

        public DeveloperController(TaskFlowDbContext context)
        {
            _context = context;
        }

        [HttpGet("my-tasks")]
        public IActionResult GetMyTasks()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized("Kullanıcı kimliği bulunamadı.");
            
            int developerId = int.Parse(userIdString);

            // Görevleri, ait oldukları Proje bilgisiyle çekiyoruz (HTML'de proje adını göstermek için)
            var myTasks = _context.Tasks
                .Include(t => t.Project)
                .Where(task => task.DeveloperId == developerId)
                .Select(t => new {
                    taskId = t.Id,
                    description = t.Description,
                    projectName = t.Project != null ? t.Project.ProjectName : "Bilinmeyen Proje",
                    status = t.Status
                })
                .ToList();

            var dashboardData = new {
                tasks = myTasks.Where(t => t.status != "BUG OLUŞTURULDU").ToList(),
                bugs = myTasks.Where(t => t.status == "BUG OLUŞTURULDU").ToList(),
                stats = new {
                    total = myTasks.Count,
                    completed = myTasks.Count(t => t.status == "TAMAMLANDI")
                }
            };

            return Ok(dashboardData);
        }

        [HttpPut("update-status")]
        public IActionResult UpdateTaskStatus([FromBody] UpdateTaskStatusDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized("Kullanıcı kimliği bulunamadı.");
            
            int developerId = int.Parse(userIdString);
            var task = _context.Tasks.FirstOrDefault(t => t.Id == dto.TaskId);

            if (task == null) return NotFound("Görev bulunamadı.");
            if (task.DeveloperId != developerId) return Forbid("Sadece size atanan görevlerin durumunu güncelleyebilirsiniz.");

            task.Status = dto.NewStatus;
            _context.SaveChanges();

            return Ok(new { message = $"Görev statüsü başarıyla '{dto.NewStatus}' olarak güncellendi." });
        }

        [HttpPost("add-comment")]
        public IActionResult AddComment([FromBody] AddCommentDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized("Kullanıcı kimliği bulunamadı.");
            
            int developerId = int.Parse(userIdString);
            var task = _context.Tasks.FirstOrDefault(t => t.Id == dto.TaskId);
            
            if (task == null) return NotFound("Görev bulunamadı.");
            if (task.DeveloperId != developerId) return Forbid("Sadece size atanan görevlere yorum yapabilirsiniz.");

            var comment = new Comment
            {
                TaskId = dto.TaskId,
                UserId = developerId,
                CommentText = dto.CommentText
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            return Ok(new { message = "Göreve yorum başarıyla eklendi." });
        }

        public class UpdateTaskStatusDto
        {
            public int TaskId { get; set; }
            public string? NewStatus { get; set; } 
        }

        public class AddCommentDto
        {
            public int TaskId { get; set; }
            public string? CommentText { get; set; } 
        }
    }
}