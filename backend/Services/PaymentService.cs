using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Payment;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class PaymentService: IPaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentResponseDto>> GetPayments(int userId)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.Date)
            .Select(p => new PaymentResponseDto
            {
                PaymentId = p.PaymentId,
                UserId = p.UserId,
                CategoryId = p.CategoryId,
                PaymentType = p.PaymentType,
                Title = p.Title,
                Description = p.Description,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Account = p.Account,
                AccountOwner = p.AccountOwner,
                Date = p.Date
            })
            .ToListAsync();
    }
    
    public async Task<IEnumerable<PaymentResponseDto>> GetIncomes(int userId)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId && p.PaymentType == "wplyw")
            .OrderByDescending(p => p.Date)
            .Select(p => new PaymentResponseDto
            {
                PaymentId = p.PaymentId,
                UserId = p.UserId,
                CategoryId = p.CategoryId,
                PaymentType = p.PaymentType,
                Title = p.Title,
                Description = p.Description,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Account = p.Account,
                AccountOwner = p.AccountOwner,
                Date = p.Date
            })
            .ToListAsync();
    }
    
    public async Task<IEnumerable<PaymentResponseDto>> GetExpenses(int userId)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId && p.PaymentType == "wydatek")
            .OrderByDescending(p => p.Date)
            .Select(p => new PaymentResponseDto
            {
                PaymentId = p.PaymentId,
                UserId = p.UserId,
                CategoryId = p.CategoryId,
                PaymentType = p.PaymentType,
                Title = p.Title,
                Description = p.Description,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Account = p.Account,
                AccountOwner = p.AccountOwner,
                Date = p.Date
            })
            .ToListAsync();
    }

    public async Task<PaymentResponseDto?> AddPayment(int userId, PaymentCreateDto dto)
    {
        var newPayment = new Payment
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            PaymentType = dto.PaymentType,
            Title = dto.Title,
            Description = dto.Description,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Account = dto.Account,
            AccountOwner = dto.AccountOwner,
            Date = dto.Date
        };

        _context.Payments.Add(newPayment);
        await _context.SaveChangesAsync();
        var newDto = new PaymentResponseDto
        {
            PaymentId = newPayment.PaymentId,
            UserId = newPayment.UserId,
            CategoryId = newPayment.CategoryId,
            PaymentType = newPayment.PaymentType,
            Title = newPayment.Title,
            Description = newPayment.Description,
            Amount = newPayment.Amount,
            PaymentMethod = newPayment.PaymentMethod,
            Account = newPayment.Account,
            AccountOwner = newPayment.AccountOwner,
            Date = newPayment.Date
        };
        return newDto;
    }
    
}