using MimeKit;
using TaRge25Shop.Core.Dto;
using Microsoft.Extensions.Configuration;
using TaRge25Shop.Core.ServiceInterface;
using MailKit.Net.Smtp;


namespace TaRge25Shop.ApplicationServices.Services
{
    public class EmailServices : IEmailServices
    {
        private readonly IConfiguration _config;

        public EmailServices
            (
                IConfiguration config
            )
        {
            _config = config;
        }

        public void SendEmail(EmailDto dto)
        {
            var email = new MimeMessage();
            // Set the sender and recipient addresses
            email.From.Add(MailboxAddress.Parse(_config.GetSection("Email:UserName").Value));
            email.To.Add(MailboxAddress.Parse(dto.To));
            email.Subject = dto.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = dto.Body
            };

            foreach (var file in dto.Attachments)
            {
                if (file.Length > 0 && file.Length < 10485760)
                {
                    using (var stream = new MemoryStream()) 
                    {
                        file.CopyTo(stream);
                        stream.Position = 0;
                        builder.Attachments.Add(file.FileName, stream.ToArray());
                    }
                }
            }
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            smtp.Connect(_config.GetSection("Email:Host").Value, 587, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(_config.GetSection("Email:UserName").Value, _config.GetSection("Email:Password").Value);
            smtp.Send(email);

        }
    }
}
