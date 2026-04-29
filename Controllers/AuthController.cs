using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ToDoList.Data;
using ToDoList.Models;
using ToDoList.Models.Dtos;
using ToDoList.Repositories;

namespace ToDoList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork; // Context yerine UnitOfWork
        private readonly IMapper _mapper;         // Manuel eşleme yerine AutoMapper
        private readonly IConfiguration _configuration;

        public AuthController(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            // Repository üzerinden kullanıcı kontrolü
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            if (allUsers.Any(u => u.Email == request.Email))
                return BadRequest("Bu e-posta adresi zaten kullanımda.");

            // AutoMapper ile DTO -> Model dönüşümü
            var user = _mapper.Map<User>(request);

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(); // Değişiklikleri kaydet

            return await GenerateAndReturnTokens(user);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            var user = allUsers.FirstOrDefault(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return BadRequest("Hatalı e-posta veya şifre.");

            return await GenerateAndReturnTokens(user);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var allUsers = await _unitOfWork.Users.GetByConditionAsync(u => u.RefreshToken == refreshToken);
            var user = allUsers.FirstOrDefault();

            if (user == null)
                return Unauthorized("Geçersiz Refresh Token.");

            if (user.TokenExpires < DateTime.Now)
                return Unauthorized("Refresh Token süresi dolmuş. Lütfen tekrar giriş yapın.");

            return await GenerateAndReturnTokens(user);
        }

        private async Task<IActionResult> GenerateAndReturnTokens(User user)
        {
            var token = CreateToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.TokenCreated = DateTime.Now;
            user.TokenExpires = DateTime.Now.AddDays(7);

            _unitOfWork.Users.Update(user); // Repository üzerinden güncelleme
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { token = token, refreshToken = refreshToken });
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(15), // Access Token'ı kısa tuttuk (Güvenlik!)
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            // Rastgele 64 karakterlik çok güvenli bir dize üretir
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}