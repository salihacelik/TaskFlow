using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TaskFlow.API.Data;
using TaskFlow.API.Models;

namespace TaskFlow.API.Controllers
{
    // MANAGER PANELİ[cite: 1]
    [Authorize(Roles = "Manager")] // Sadece yöneticiler girebilir
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly TaskFlowDbContext _context;

        public ManagerController(TaskFlowDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. PROJE YÖNETİMİ: Oluştur / Güncelle / Sil[cite: 1]
        // ==========================================

        [HttpPost("create-project")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ProjectName))
                return BadRequest("Proje adı boş olamaz.");

            var newProject = new Project
            {
                ProjectName = dto.ProjectName,
                Description = dto.Description
            };

            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();
            
            return Ok(new { Message = "Proje başarıyla oluşturuldu.", ProjectId = newProject.ProjectId });
        }

        [HttpPut("update-project/{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectDto dto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound("Güncellenecek proje bulunamadı.");

            project.ProjectName = dto.ProjectName;
            project.Description = dto.Description;

            await _context.SaveChangesAsync();
            return Ok("Proje başarıyla güncellendi.");
        }

        [HttpDelete("delete-project/{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound("Silinecek proje bulunamadı.");

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            
            return Ok("Proje sistemden başarıyla silindi.");
        }

        // ==========================================
        // 2. GÖREV ATA: Developer'a Görev Ata[cite: 1]
        // ==========================================

        [HttpPost("assign-task")]
        public async Task<IActionResult> AssignTask([FromBody] AssignTaskDto dto)
        {
            // Görev Yaşam Döngüsü kuralı: Yeni görevler her zaman "ATANDI" statüsüyle başlar[cite: 1].
            var task = new TaskItem
            {
                ProjectId = dto.ProjectId,
                DeveloperId = dto.DeveloperId,
                Description = dto.Description,
                Status = "ATANDI" 
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            
            return Ok(new { Message = "Görev başarıyla atandı (ATANDI statüsünde).", TaskId = task.TaskId });
        }

        // ==========================================
        // 3. DASHBOARD: İstatistikleri Görüntüle[cite: 1]
        // ==========================================

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            // Sistemin analiz verilerini tek seferde veritabanından topluyoruz.
            var totalProjects = await _context.Projects.CountAsync();
            
            // "COMPLETED" (Onaylandı) ve "BUG OLUŞTURULDU" statüsündeki görevleri sayıyoruz[cite: 1].
            var completedTasks = await _context.Tasks.CountAsync(t => t.Status == "COMPLETED");
            var totalBugs = await _context.Tasks.CountAsync(t => t.Status == "BUG OLUŞTURULDU");

            return Ok(new 
            { 
                Message = "Dashboard verileri başarıyla getirildi.",
                TotalProjects = totalProjects,
                CompletedTasks = completedTasks,
                TotalBugs = totalBugs
            });
        }
    }

    // --- DTO SINIFLARI (Temiz Veri Aktarımı İçin) ---
    
    public class ProjectDto
    {
        public string ProjectName { get; set; }
        public string? Description { get; set; }
    }

    public class AssignTaskDto
    {
        public int ProjectId { get; set; }
        public int DeveloperId { get; set; }
        public string Description { get; set; }
    }
}