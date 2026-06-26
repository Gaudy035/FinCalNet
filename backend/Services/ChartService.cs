using backend.Data;
using backend.DTOs.Charts;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ChartsService: IChartsService
{
    private readonly AppDbContext _context;

    public ChartsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StatsResponseDto>> GetStats(int userId)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId && p.PaymentType == "wydatek" && p.CategoryId != null)
            .GroupBy(p => p.Category!.CategoryName)
            .Select(s => new StatsResponseDto
            {
                CategoryName = s.Key,
                Amount = s.Sum(t => t.Amount)
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<SummaryResponseDto>> GetSummary(int userId)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .GroupBy(p => p.PaymentType)
            .Select(s => new SummaryResponseDto
            {
                TypeName = s.Key,
                Amount = s.Sum(t => t.Amount)
            })
            .ToListAsync();
    }
}