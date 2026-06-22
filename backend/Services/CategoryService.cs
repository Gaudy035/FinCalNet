using backend.Data;
using backend.DTOs.Category;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class CategoryService: ICategoryService
{
    private readonly AppDbContext _conetxt;

    public CategoryService(AppDbContext context)
    {
        _conetxt = context;
    }

    public async Task<IEnumerable<CategoryDto>> GetCategories()
    {
        return await _conetxt.Categories
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
            }).ToListAsync();
    }
}