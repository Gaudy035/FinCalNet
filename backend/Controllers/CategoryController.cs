using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
public class CategoryController: ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("kategorie")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetCategories();
        if (!categories.Any())
        {
            return NotFound("Bład przy pobieraniu kategorii");
        }
        return Ok(categories);
    }
}