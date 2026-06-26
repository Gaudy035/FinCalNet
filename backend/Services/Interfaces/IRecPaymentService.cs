using backend.DTOs.RecPayment;

namespace backend.Services;

public interface IRecPaymentService
{
    Task<IEnumerable<RecPaymentResponseDto>> GetRecPayments(int userId);

    Task<RecPaymentResponseDto?> AddRecPayment(int userId, RecPaymentCreateDto dto);

    Task<RecPaymentResponseDto?> ModifyRecPayment(int recPaymentId, int userId, RecPaymentModifyDto dto);
}