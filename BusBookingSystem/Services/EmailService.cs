using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace BusBookingSystem.Services;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    private async Task SendEmailAsync(string email, string subject, string htmlBody)
    {
        var smtpHost = _configuration["Smtp:Host"];
        var smtpPort = int.TryParse(_configuration["Smtp:Port"], out var port) ? port : 587;
        var smtpUser = _configuration["Smtp:Username"];
        var smtpPass = _configuration["Smtp:Password"];
        var fromAddress = _configuration["Smtp:From"] ?? "no-reply@busbooking.com";

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(fromAddress));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
        {
            _logger.LogInformation("SMTP not configured. Email would be sent to {Email} with subject {Subject}", email, subject);
            _logger.LogInformation(htmlBody);
            return;
        }

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(smtpUser, smtpPass);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendApprovalEmailAsync(string email, string name, string status, string reason = "")
    {
        var subject = status == "approved" ? "Bus operator application approved" : "Bus operator application rejected";
        var body = $"<p>Hi {name},</p><p>Your bus operator request has been <strong>{status}</strong>.</p>";
        if (!string.IsNullOrWhiteSpace(reason)) body += $"<p>Reason: {reason}</p>";
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendCancellationEmailAsync(string email, string name, string busRegistration, string reason, decimal refundAmount)
    {
        var subject = "Bus cancellation and refund details";
        var body = $"<p>Hi {name},</p><p>Your bus {busRegistration} has been cancelled.</p><p>Reason: {reason}</p><p>Refund: {refundAmount:C}</p>";
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendOperatorCancellationEmailAsync(string email, string name, string busRegistration, string reason, int bookingCount, decimal totalRefund)
    {
        var subject = "Bus cancellation notice";
        var body = $"<p>Hi {name},</p><p>Your bus {busRegistration} has been cancelled for {bookingCount} bookings.</p><p>Reason: {reason}</p><p>Total refund amount: {totalRefund:C}</p>";
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendBookingConfirmationEmailAsync(string email, string name, string busRegistration, string source, string destination, string seatNumber, decimal amount)
    {
        var subject = "Booking confirmed";
        var body = $"<p>Hi {name},</p><p>Your booking on {busRegistration} from {source} to {destination} is confirmed.</p><p>Seats: {seatNumber}</p><p>Amount: {amount:C}</p>";
        await SendEmailAsync(email, subject, body);
    }
}
