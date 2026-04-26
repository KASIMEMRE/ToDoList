using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ToDoList.Data;
using ToDoList.Models;
using ToDoList.Models.Dtos;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Bu sınıfa girmek için anahtar (token) şart!
public class TodosController : ControllerBase
{
    private readonly AppDbContext _context;

    public TodosController(AppDbContext context)
    {
        _context = context;
    }

    // Yardımcı Metod: Token içindeki NameIdentifier'ı (UserId) okur
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // 1. GET: Tüm Görevleri Listele (Sayfalama ve Filtreleme ile)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isCompleted, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var userId = GetUserId();
        var query = _context.Todos.Where(t => t.UserId == userId).AsQueryable();

        // Filtre: Sadece tamamlananları veya tamamlanmayanları getir
        if (isCompleted.HasValue)
            query = query.Where(t => t.IsCompleted == isCompleted.Value);

        var total = await query.CountAsync();
        var data = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

        return Ok(new { data, page, limit, total });
    }

    // 2. GET: Tek Bir Görevin Detayı
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null) return NotFound(new { message = "Görev bulunamadı." });
        return Ok(todo);
    }

    // 3. POST: Yeni Görev Ekle
    [HttpPost]
    public async Task<IActionResult> Create(TodoCreateDto dto)
    {
        var todo = new Todo
        {
            Title = dto.Title,
            Description = dto.Description,
            UserId = GetUserId() // Token'dan gelen ID'yi atıyoruz
        };

        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
    }

    // 4. PUT: Görev Güncelle (Güvenlik Kontrollü)
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TodoCreateDto dto)
    {
        var userId = GetUserId();
        // Sadece bana ait olan görevi bul
        var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null) return StatusCode(403, new { message = "Bu görevi güncelleme yetkiniz yok!" });

        todo.Title = dto.Title;
        todo.Description = dto.Description;
        // todo.IsCompleted = true; // İstersen bunu da DTO'ya ekleyip güncelleyebilirsin

        await _context.SaveChangesAsync();
        return Ok(todo);
    }

    // 5. DELETE: Görev Sil (Güvenlik Kontrollü)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null) return StatusCode(403, new { message = "Bu görevi silme yetkiniz yok!" });

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();
        return NoContent(); // 204 döner
    }
}