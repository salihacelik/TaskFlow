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
    [Authorize(Roles = "Tester")] // Sadece Tester rolüne sahip olanlar girebilir
    public class TesterController : ControllerBase
    {
        private readonly TaskFlowDbContext _context;

        public TesterController(TaskFlowDbContext context)
        {
            _context = context;
        }

        // 1. TESTE HAZIR GÖREVLERİ GÖRÜNTÜLE
        [HttpGet("ready-for-test")]
        public async Task<IActionResult> GetReadyForTestTasks()
        {
            // Sistemdeki tester'ın kimliğini çekiyoruz
            var testerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int parsedTesterId = int.Parse(testerIdString ?? "0");

            // Sadece bu tester'a atanmış ve durumu 'READY FOR TEST' olan görevleri getir
            var readyTasks = await _context.Tasks
                .Where(t => t.Status == "READY FOR TEST" && t.TesterId == parsedTesterId)
                .ToListAsync();

            if (!readyTasks.Any())
            {
                return NotFound("Size atanmış ve test edilmeyi bekleyen bir görev bulunmuyor.");
            }

            return Ok(readyTasks);
        }

        // 2. BAŞARILIYSA ONAYLA, DEĞİLSE BUG'A DÜŞÜR VE YORUM EKLE
        [HttpPut("evaluate-task")]
        public async Task<IActionResult> EvaluateTask([FromBody] EvaluateTaskDto dto)
        {
            // MANTIK HATASI ÇÖZÜMÜ: Sadece TaskId'ye göre nokta atışı arama yapıyoruz.
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == dto.TaskId);
            
            if (task == null) return NotFound("Görev bulunamadı.");

            var testerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int parsedTesterId = int.Parse(testerIdString ?? "0");

            // Güvenlik Duvarı: Bu görev gerçekten isteği atan test uzmanına mı ait?
            if (task.TesterId != parsedTesterId)
            {
                return StatusCode(403, "Bu görevi değerlendirme yetkiniz yok.");
            }

            // Görev Yaşam Döngüsü kurallarını işletiyoruz
            if (dto.IsSuccessful)
            {
                task.Status = "COMPLETED"; // Test başarılıysa onaylandı statüsüne geçer
            }
            else
            {
                task.Status = "BUG OLUŞTURULDU"; // Test başarısızsa Developer'a geri döner
            }

            // Değerlendirme sırasında bir açıklama/yorum yazıldıysa veritabanına kaydet
            if (!string.IsNullOrEmpty(dto.CommentText))
            {
                var comment = new Comment 
                {
                    TaskId = dto.TaskId,
                    UserId = parsedTesterId,
                    CommentText = dto.CommentText // Comment.cs ile birebir uyumlu
                };
                _context.Comments.Add(comment);
            }

            await _context.SaveChangesAsync();
            return Ok($"Görev değerlendirmesi tamamlandı. Yeni Durum: {task.Status}");
        }

        // 3. SADECE YORUM EKLE
        [HttpPost("add-comment")]
        public async Task<IActionResult> AddComment([FromBody] TesterCommentDto dto)
        {
            var testerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int parsedTesterId = int.Parse(testerIdString ?? "0");
            
            // Mantıksal doğrulama: Yorum yapılmak istenen görev ID'si mevcut mu?
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == dto.TaskId);
            if (task == null) return NotFound("Görev bulunamadı.");

            var comment = new Comment
            {
                TaskId = dto.TaskId,
                UserId = parsedTesterId,
                CommentText = dto.CommentText ?? "" 
            };
            
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            
            return Ok("Yorum başarıyla eklendi.");
        }
    }

    // --- DTO SINIFLARI ---
    
    public class EvaluateTaskDto
    {
        public int TaskId { get; set; }
        public bool IsSuccessful { get; set; } // true ise Onayla, false ise Bug'a düşür
        public string? CommentText { get; set; } // Opsiyonel değerlendirme notu
    }

    public class TesterCommentDto
    {
        public int TaskId { get; set; }
        public string? CommentText { get; set; }
    }
}