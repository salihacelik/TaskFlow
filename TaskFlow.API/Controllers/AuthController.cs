using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Data;
using TaskFlow.API.Models;
using TaskFlow.API.DTOs;
using System.Linq;

namespace TaskFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TaskFlowDbContext _context;

        public AuthController(TaskFlowDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto request)
        {
            // 1. Email sistemde var mı kontrolü
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest("Bu email adresi zaten kullanılıyor.");
            }

            // 2. Yeni kullanıcıyı oluştur ve veritabanına ekle
            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = request.Password, // İlerleyen adımlarda güvenlik için bu şifreyi Hash'leyeceğiz.
                Role = request.Role
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new { message = "Kayıt işlemi başarılı! Sisteme giriş yapabilirsiniz." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto request)
        {
            // 1. Kullanıcıyı veritabanında e-posta adresine göre bul
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            // 2. Kullanıcı yoksa veya şifre eşleşmiyorsa hata döndür
            // Not: Gerçek projelerde şifreler her zaman hashlenerek (şifrelenerek) kontrol edilir, şimdilik temel mantığı kuruyoruz.
            if (user == null || user.PasswordHash != request.Password)
            {
                return Unauthorized("Geçersiz e-posta veya şifre.");
            }

            // 3. İleride eklenecek olan JWT Token (Dijital Anahtar) Üretim Alanı
            // Kullanıcının rolü burada token'a işlenecek.
                
            return Ok(new { Message = "Giriş başarılı!", Role = user.Role });
        }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}