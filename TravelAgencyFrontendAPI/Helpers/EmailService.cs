using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using TravelAgencyFrontendAPI.Helpers;

public class EmailService
{
    private readonly SmtpSettings _smtp;

    public EmailService(IOptions<SmtpSettings> smtpOptions)
    {
        _smtp = smtpOptions.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpClient = new SmtpClient(_smtp.Host)
        {
            Port = _smtp.Port,
            Credentials = new NetworkCredential(_smtp.FromEmail, _smtp.AppPassword),
            EnableSsl = true
        };

        var mail = new MailMessage
        {
            From = new MailAddress(_smtp.FromEmail, "嶼你同行客服中心"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(toEmail);

        await smtpClient.SendMailAsync(mail);
    }
}