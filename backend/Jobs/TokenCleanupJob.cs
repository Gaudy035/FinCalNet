using backend.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace backend.Jobs;

public class TokenCleanupJob: IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TokenCleanupJob> _logger;

    public TokenCleanupJob(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[{time}] Czyszczenie tokenow", DateTime.UtcNow);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dateToRemove = DateTimeOffset.UtcNow.AddDays(-1);

        var tokensToRemove = await dbContext.RefreshTokens
            .Where(rt => rt.ExpiresAt <= dateToRemove || rt.RevokedAt <= dateToRemove)
            .ToListAsync();

        var tokenCount = tokensToRemove.Count;

        if (tokenCount == 0)
        {
            _logger.LogInformation("Brak tokenow do usuniecia");
            return;
        }
        
        dbContext.RemoveRange(tokensToRemove);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Usunieto {count} tokenow", tokenCount);
    }
}