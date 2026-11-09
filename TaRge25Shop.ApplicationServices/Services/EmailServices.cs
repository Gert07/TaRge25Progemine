using MimeKit;
using TaRge25Shop.Core.Dto;
using Microsoft.Extensions.Configuration;

namespace TaRge25Shop.ApplicationServices.Services
{
    public class EmailServices
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
            email.From.Add(MailboxAddress.Parse(_config.GetSection("Email:Username").Value));
            email.To.Add(MailboxAddress.Parse(dto.To));
            email.Subject = dto.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = dto.Body
            };
        }
    }
}
