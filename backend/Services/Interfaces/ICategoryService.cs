using backend.DTOs.Category;

namespace backend.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetCategories();
}