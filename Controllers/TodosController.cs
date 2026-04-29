using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ToDoList.Data;
using ToDoList.Models;
using ToDoList.Models.Dtos;
using ToDoList.Repositories;

[EnableRateLimiting("sabit")]
[Route("api/[controller]")]
[ApiController]
[Authorize] 
public class TodosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TodosController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }


    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    
    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromQuery] string? search,       
    [FromQuery] bool? isCompleted,    
    [FromQuery] string? sortBy,       
    [FromQuery] int page = 1,
    [FromQuery] int limit = 10)
    {
        var userId = GetUserId();

        var query = (await _unitOfWork.Todos.GetByConditionAsync(t => t.UserId == userId)).AsQueryable();

        
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(t => t.Title.Contains(search));
        }

        
        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }

        query = sortBy?.ToLower() switch
        {
            "title" => query.OrderBy(t => t.Title),
            "id" => query.OrderByDescending(t => t.Id),
            _ => query.OrderByDescending(t => t.Id)
        };

       
        var total = await query.CountAsync();
        var data = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

        return Ok(new { data, page, limit, total });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        var todo = (await _unitOfWork.Todos.GetByConditionAsync(t => t.Id == id && t.UserId == userId)).FirstOrDefault();

        if (todo == null) return NotFound(new { message = "Görev bulunamadı." });
        return Ok(todo);
    }

    
    [HttpPost]
    public async Task<IActionResult> Create(TodoCreateDto dto)
    {
        var todo = new Todo
        {
            Title = dto.Title,
            Description = dto.Description,
            UserId = GetUserId()
        };

        await _unitOfWork.Todos.AddAsync(todo);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
    }

    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TodoCreateDto dto)
    {
        var userId = GetUserId();
        
        var todo = (await _unitOfWork.Todos.GetByConditionAsync(t => t.Id == id && t.UserId == userId)).FirstOrDefault();

        if (todo == null) return StatusCode(403, new { message = "Bu görevi güncelleme yetkiniz yok!" });

        todo.Title = dto.Title;
        todo.Description = dto.Description;
        
        await _unitOfWork.SaveChangesAsync();
        return Ok(todo);
    }

    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var todo = (await _unitOfWork.Todos.GetByConditionAsync(t => t.Id == id && t.UserId == userId)).FirstOrDefault();

        if (todo == null) return StatusCode(403, new { message = "Bu görevi silme yetkiniz yok!" });

        _unitOfWork.Todos.Delete(todo);
        await _unitOfWork.SaveChangesAsync();
        return NoContent(); 
    }
}