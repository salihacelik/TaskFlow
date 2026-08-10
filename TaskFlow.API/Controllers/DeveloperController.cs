using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.API.Controllers
{
 [Authorize(Roles = "Developer")]
    [Route("api/[controller]")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        [HttpGet("my-tasks")]
        public IActionResult GetMyTasks()
        {
            // Görevler: Kendisine Atanan Görevleri Görüntüler[cite: 1]
            return Ok("Size atanan görevler listelendi.");
        }

        [HttpPut("update-status")]
        public IActionResult UpdateTaskStatus()
        {
            // Durum Güncelle: Görevlerin durumlarını günceller (IN PROGRESS vb.)[cite: 1]
            return Ok("Görev durumu güncellendi.");
        }

        [HttpPost("add-comment")]
        public IActionResult AddComment()
        {
            // Yorum Ekle: Göreve yorum yapabilir[cite: 1]
            return Ok("Göreve yorum eklendi.");
        }
    }
}