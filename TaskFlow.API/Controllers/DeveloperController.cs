using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using TaskFlow.API.Data; // DbContext'in bulunduğu namespace
using TaskFlow.API.Models;

namespace TaskFlow.API.Controllers
{
    [Authorize(Roles = "Developer")]
    [Route("api/[controller]")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        private readonly TaskFlowDbContext _context;

        // SQL Server ile iletişim kuracak DbContext'i sınıfa dahil ediyoruz
        public DeveloperController(TaskFlowDbContext context)
        {
            _context = context;
        }

        [HttpGet("my-tasks")]
        public IActionResult GetMyTasks()
        {
            // 1. JWT Token içerisinden sisteme giriş yapan Developer'ın ID'sini yakala
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userIdString)) 
            {
                return Unauthorized("Kullanıcı kimliği bulunamadı.");
            }
            
            int developerId = int.Parse(userIdString);

            // 2. Veritabanındaki Tasks (Görevler) tablosundan sadece bu ID'ye sahip görevleri filtrele
            // Developer, kendi üzerine atanan görevleri görüntüler[cite: 1].
            var myTasks = _context.Tasks
                .Where(task => task.DeveloperId == developerId)
                .ToList();

            return Ok(myTasks);
        }

        [HttpPut("update-status")]
        public IActionResult UpdateTaskStatus([FromBody] UpdateTaskStatusDto dto)
        {
            // 1. İşlemi yapan Developer'ın kimliğini yakala
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Kullanıcı kimliği bulunamadı.");
            }
            int developerId = int.Parse(userIdString);

            // 2. Veritabanından güncellenecek görevi bul
            var task = _context.Tasks.FirstOrDefault(t => t.Id == dto.TaskId);

            if (task == null)
            {
                return NotFound("Görev bulunamadı.");
            }

            // 3. Güvenlik Kontrolü: Bu görev gerçekten bu Developer'a mı atanmış?
            if (task.DeveloperId != developerId)
            {
                return Forbid("Sadece size atanan görevlerin durumunu güncelleyebilirsiniz.");
            }

            // 4. Durum Güncelle: Görevlerin durumlarını günceller[cite: 1]
            task.Status = dto.NewStatus;
            
            // 5. Değişiklikleri veritabanına kaydet
            _context.SaveChanges();

            return Ok($"Görev statüsü başarıyla '{dto.NewStatus}' olarak güncellendi.");
        }



        [HttpPost("add-comment")]
        public IActionResult AddComment([FromBody] AddCommentDto dto)
        {
            // 1. İşlemi yapan Developer'ın kimliğini yakala
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Kullanıcı kimliği bulunamadı.");
            }
            int developerId = int.Parse(userIdString);

            // 2. Veritabanından yorum yapılacak görevi bul
            var task = _context.Tasks.FirstOrDefault(t => t.Id == dto.TaskId);
            if (task == null)
            {
                return NotFound("Görev bulunamadı.");
            }

            // 3. Güvenlik Kontrolü: Bu görev gerçekten bu Developer'a mı atanmış?
            if (task.DeveloperId != developerId)
            {
                return Forbid("Sadece size atanan görevlere yorum yapabilirsiniz.");
            }

            // 4. Yeni yorum nesnesini oluştur
            // TaskFlow.pdf'e göre: YORUM EKLE (Göreve Yorum Yapabilir)[cite: 1]
            var comment = new Comment
            {
                TaskId = dto.TaskId,
                UserId = developerId,
                CommentText = dto.CommentText
            };

            // 5. Veritabanına kaydet
            _context.Comments.Add(comment);
            _context.SaveChanges();

            return Ok("Göreve yorum başarıyla eklendi.");
        }

        // DTO (Data Transfer Object) Sınıfları
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