using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.API.Controllers
{
  [Authorize(Roles = "Tester")]
    [Route("api/[controller]")]
    [ApiController]
    public class TesterController : ControllerBase
    {
        [HttpGet("ready-for-test")]
        public IActionResult GetTasksReadyForTest()
        {
            // Teste Hazır Görevler: Test için hazır görevleri görüntüler[cite: 1]
            return Ok("Teste hazır görevler listelendi.");
        }

        [HttpPost("approve-task")]
        public IActionResult ApproveTask()
        {
            // Başarılıysa Onay (COMPLETED statüsü)[cite: 1]
            return Ok("Görev testten geçti ve onaylandı.");
        }

        [HttpPost("report-bug")]
        public IActionResult ReportBug()
        {
            // Değilse BUG'A DÜŞÜR, Yorum Ekle, Developer'a Gönder[cite: 1]
            return Ok("Bug oluşturuldu ve görev Developer'a geri gönderildi.");
        }
        
        [HttpPost("add-comment")]
        public IActionResult AddComment()
        {
            // Yorum Ekle: Göreve yorum yapabilir[cite: 1]
            return Ok("Teste dair yorum eklendi.");
        }
    }
}