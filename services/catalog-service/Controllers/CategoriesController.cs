using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Catalog.Api.Data;
using Pos.Catalog.Api.Domain;
using Pos.Catalog.Api.Dtos;
using Pos.Catalog.Api.Security;

namespace Pos.Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly CatalogDbContext _db;

    public CategoriesController(CatalogDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Manager)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryRequest request)
    {
        if (await _db.Categories.AnyAsync(c => c.Name == request.Name))
        {
            return Conflict($"A category named '{request.Name}' already exists.");
        }

        var category = new Category { Name = request.Name, Description = request.Description };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), new CategoryDto(category.Id, category.Name, category.Description));
    }
}
