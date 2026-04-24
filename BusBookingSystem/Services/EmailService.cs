namespace BusBookingSystem.Services;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendApprovalEmailAsync(string email, string name, string status, string reason = "")
    {
        _logger.LogInformation($"Sending approval email to {email}");
        await Task.CompletedTask;
    }

    public async Task SendCancellationEmailAsync(string email, string name, string busRegistration, string reason, decimal refundAmount)
    {
        _logger.LogInformation($"Sending cancellation email to {email}");
        await Task.CompletedTask;
    }

    public async Task SendOperatorCancellationEmailAsync(string email, string name, string busRegistration, string reason, int bookingCount, decimal totalRefund)
    {
        _logger.LogInformation($"Sending operator cancellation email to {email}");
        await Task.CompletedTask;
    }

    public async Task SendBookingConfirmationEmailAsync(string email, string name, string busRegistration, string source, string destination, string seatNumber, decimal amount)
    {
        _logger.LogInformation($"Sending booking confirmation to {email}");
        await Task.CompletedTask;
    }
}
