using backend.Data;
using backend.Data.Entities;
using backend.DTOs.RecPayment;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class RecPaymentService: IRecPaymentService
{
    private readonly AppDbContext _context;

    public RecPaymentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RecPaymentResponseDto>> GetRecPayments(int userId)
    {
        return await _context.RecPayments
            .Where(rp => rp.UserId == userId)
            .OrderBy(rp => rp.NextDate)
            .Select(rp => new RecPaymentResponseDto
            {
                UserId = rp.UserId,
                CategoryId = rp.CategoryId,
                RecPaymentId = rp.RecPaymentId,
                PaymentType = rp.PaymentType,
                Title = rp.Title,
                Description = rp.Description,
                Amount = rp.Amount,
                PaymentMethod = rp.PaymentMethod,
                Account = rp.Account,
                AccountOwner = rp.AccountOwner,
                Interval = rp.Interval,
                NextDate = rp.NextDate,
                IsActive = rp.IsActive
            })
            .ToListAsync();
    }

    public async Task<RecPaymentResponseDto?> AddRecPayment(int userId, RecPaymentCreateDto dto)
    {
        var newRecPayment = new RecPayment
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            PaymentType = dto.PaymentType,
            Title = dto.Title,
            Description = dto.Description,
            PaymentMethod = dto.PaymentMethod,
            Amount = dto.Amount,
            Account = dto.Account,
            AccountOwner = dto.AccountOwner,
            Interval = dto.Interval,
            NextDate = dto.NextDate,
            IsActive = true,
        };

        _context.RecPayments.Add(newRecPayment);
        await _context.SaveChangesAsync();
        
        return new RecPaymentResponseDto
        {
            UserId = newRecPayment.UserId,
            CategoryId = newRecPayment.CategoryId,
            RecPaymentId = newRecPayment.RecPaymentId,
            PaymentType = newRecPayment.PaymentType,
            Title = newRecPayment.Title,
            Description = newRecPayment.Description,
            Amount = newRecPayment.Amount,
            PaymentMethod = newRecPayment.PaymentMethod,
            Account = newRecPayment.Account,
            AccountOwner = newRecPayment.AccountOwner,
            Interval = newRecPayment.Interval,
            NextDate = newRecPayment.NextDate,
            IsActive = newRecPayment.IsActive
        };
    }

    public async Task<RecPaymentResponseDto?> ModifyRecPayment(int recPaymentId, int userId, RecPaymentModifyDto dto)
    {
        var recPayment = await _context.RecPayments.FindAsync(recPaymentId);

        if(recPayment == null)
        {
            return null;
        }

        recPayment.CategoryId = dto.CategoryId;
        recPayment.PaymentType = dto.PaymentType;
        recPayment.Title = dto.Title;
        recPayment.Description = dto.Description;
        recPayment.Amount = dto.Amount;
        recPayment.PaymentMethod = dto.PaymentMethod;
        recPayment.Account = dto.Account;
        recPayment.AccountOwner = dto.AccountOwner;
        recPayment.Interval = dto.Interval;
        recPayment.NextDate = dto.NextDate;
        recPayment.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return new RecPaymentResponseDto
        {
            UserId = recPayment.UserId,
            CategoryId = recPayment.CategoryId,
            RecPaymentId = recPayment.RecPaymentId,
            PaymentType = recPayment.PaymentType,
            Title = recPayment.Title,
            Description = recPayment.Description,
            Amount = recPayment.Amount,
            PaymentMethod = recPayment.PaymentMethod,
            Account = recPayment.Account,
            AccountOwner = recPayment.AccountOwner,
            Interval = recPayment.Interval,
            NextDate = recPayment.NextDate,
            IsActive = recPayment.IsActive
        };
    }
}