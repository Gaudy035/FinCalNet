using backend.Data;
using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace backend.Jobs;

public class RecPaymentJob: IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecPaymentJob> _logger;

    public RecPaymentJob(IServiceProvider serviceProvider, ILogger<RecPaymentJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private static DateOnly CalcNextDate(DateOnly current, string interval)
    {
        return interval switch
        {
            "P7D" => current.AddDays(7),
            "P30D" => current.AddMonths(1),
            "P1Y" => current.AddYears(1),
            _ => throw new ArgumentException("Nieprawidlowy interval")
        };
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[{Time}] Sprawdzanie transakcji cyklicznych", DateTime.Now);

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var duePayments = await dbContext.RecPayments
            .Where(rp => rp.NextDate <= today && rp.IsActive)
            .ToListAsync();
        
        foreach (var rp in duePayments)
        {
            try
            {
                var nextDate = CalcNextDate(rp.NextDate, rp.Interval);

                var newPayment = new Payment
                {
                    UserId = rp.UserId,
                    CategoryId = rp.CategoryId,
                    PaymentType = rp.PaymentType,
                    Title = rp.Title,
                    Description = rp.Description,
                    Amount = rp.Amount,
                    PaymentMethod = rp.PaymentMethod,
                    Account = rp.Account,
                    AccountOwner = rp.AccountOwner,
                    Date = rp.NextDate
                };

                dbContext.Payments.Add(newPayment);
                rp.NextDate = nextDate;

                _logger.LogInformation("Dodano transakcje {Id} -- {Title} dla uzytkownika {UserId}, nastepny termin: {NextDate}",
                    rp.RecPaymentId, rp.Title, rp.UserId, nextDate);
                
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wystapil blad dla transakjci {Id} -- {Title}", rp.RecPaymentId, rp.Title);
            }
        }
    }
}