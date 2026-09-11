using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.DTOs;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Persistence;

namespace MyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IValidator<CreateCategoryDto> _validator;

    public CategoriesController(AppDbContext context, IValidator<CreateCategoryDto> validator)
    {
        _context = context;
        _validator = validator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var categories = await _context.Categories
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        return Ok(new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
    {
        await _validator.ValidateAndThrowAsync(dto);

        var category = new Category { Name = dto.Name, Description = dto.Description };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var result = new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description };
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CreateCategoryDto dto)
    {
        await _validator.ValidateAndThrowAsync(dto);

        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        category.Name = dto.Name;
        category.Description = dto.Description;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
