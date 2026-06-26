using backend.DTOs.Charts;

namespace backend.Services;

public interface IChartsService
{
    Task<IEnumerable<StatsResponseDto>> GetStats(int userId);

    Task<IEnumerable<SummaryResponseDto>> GetSummary(int userId);
}