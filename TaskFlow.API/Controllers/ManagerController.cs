using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.API.Controllers
{
    // MANAGER PANELİ[cite: 1]
    [Authorize(Roles = "Manager")]
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        [HttpPost("create-project")]
        public IActionResult CreateProject()
        {
            // Proje Yönetimi: Oluştur / Güncelle / Sil[cite: 1]
            return Ok("Proje başarıyla oluşturuldu.");
        }

        [HttpPost("assign-task")]
        public IActionResult AssignTask()
        {
            // Developer'a Görev Ata[cite: 1]
            return Ok("Görev atandı (YENİ GÖREV statüsünde).");
        }
        
        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            // Dashboard: Toplam Proje, Tamamlanan, Toplam Bug verileri[cite: 1]
            return Ok("Dashboard verileri getirildi.");
        }
    }
}