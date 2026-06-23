using backend.DTOs.Payment;

namespace backend.Services;

public interface IPaymentService
{
    Task<IEnumerable<PaymentResponseDto>> GetPayments(int userId);

    Task<IEnumerable<PaymentResponseDto>> GetIncomes(int userId);

    Task<IEnumerable<PaymentResponseDto>> GetExpenses(int userId);

    Task<PaymentResponseDto?> AddPayment(int userId, PaymentCreateDto dto);
}