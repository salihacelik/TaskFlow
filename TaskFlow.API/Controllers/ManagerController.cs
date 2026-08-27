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
                Description = dto.Description,
                Status = "Aktif"

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
// 3. GELİŞMİŞ DASHBOARD: Tüm İstatistikleri ve Kadroyu Görüntüle
// ==========================================

[HttpGet("dashboard")]
public async Task<IActionResult> GetDashboard()
{
    // 1. PROJE LİSTESİ: Tüm projelerin adları ve güncel durumları
    var projectsList = await _context.Projects
        .Select(p => new { 
            ProjectName = p.ProjectName, 
            Status = p.Status 
        })
        .ToListAsync();

    // 2. KADRO (GELİŞTİRİCİLER VE TEST UZMANLARI)
    // Sadece rolü "Developer" olan kullanıcıları çekiyoruz
    var developers = await _context.Users
        .Where(u => u.Role == "Developer")
        .Select(u => new { 
            u.UserId, 
            u.Username, 
            u.Email 
        })
        .ToListAsync();

    // Sadece rolü "Tester" olan kullanıcıları çekiyoruz
    var testers = await _context.Users
        .Where(u => u.Role == "Tester")
        .Select(u => new { 
            u.UserId, 
            u.Username, 
            u.Email 
        })
        .ToListAsync();

    // 3. PROJE VE GÖREV İSTATİSTİKLERİ
    // Proje bazlı metrikler (Yapılmakta Olan vs Yapılan)
    var ongoingProjectsCount = await _context.Projects.CountAsync(p => p.Status == "Aktif");
    var completedProjectsCount = await _context.Projects.CountAsync(p => p.Status != "Aktif"); 
    
    // TaskFlow kuralına uygun görev (Task) metrikleri
    var totalProjects = projectsList.Count;
    var completedTasks = await _context.Tasks.CountAsync(t => t.Status == "COMPLETED");
    var totalBugs = await _context.Tasks.CountAsync(t => t.Status == "BUG OLUŞTURULDU");

    // Tüm verileri tek bir JSON objesi (DTO mantığı) ile yöneticiye sunuyoruz
    return Ok(new 
    { 
        Message = "Gelişmiş Dashboard verileri başarıyla getirildi.",
        
        // Üst Kısım: İstatistikler
        Statistics = new {
            Projects = new {
                Total = totalProjects,
                Ongoing = ongoingProjectsCount,
                Completed = completedProjectsCount
            },
            Tasks = new {
                CompletedTasks = completedTasks,
                TotalBugs = totalBugs
            }
        },

        // Orta Kısım: Proje Listesi
        ProjectsOverview = projectsList,

        // Alt Kısım: Ekip Kadrosu
        Team = new {
            Developers = developers,
            Testers = testers
        }
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
