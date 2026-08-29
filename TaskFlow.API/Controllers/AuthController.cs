using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // appsettings'i okumak için gerekli
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using TaskFlow.API.Data;
// using TaskFlow.API.DTOs; // Kendi dosyanda DTO'ları aşağıya eklediğin için bunu yoruma aldım, çakışma yapmasın
using TaskFlow.API.Models;

namespace TaskFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TaskFlowDbContext _context;
        private readonly IConfiguration _configuration; // appsettings bağlantısı eklendi

        public AuthController(TaskFlowDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest("Bu email adresi zaten kullanılıyor.");
            }

            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = request.Password, 
                Role = request.Role
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new { message = "Kayıt işlemi başarılı! Sisteme giriş yapabilirsiniz." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null || user.PasswordHash != request.Password)
            {
                return Unauthorized("Geçersiz e-posta veya şifre.");
            }

            // JWT Token (Dijital Anahtar) Üretimi ve Yetkiye Göre Yönlendirme Altyapısı
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Anahtarı artık sabit kısımdan DEĞİL, doğrudan appsettings.json'dan alıyoruz!
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role) // Sistem bunu [Authorize(Roles="Manager")] ile eşleştirebilir
                }),
                Expires = DateTime.UtcNow.AddDays(30), // Rahat çalışman için ömrünü 30 gün yaptık
                Issuer = _configuration["Jwt:Issuer"],     // appsettings'ten okundu
                Audience = _configuration["Jwt:Audience"], // appsettings'ten okundu
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString, Role = user.Role, Message = "Giriş başarılı!" });
        }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    
    public class RegisterDto 
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}