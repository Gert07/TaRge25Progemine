using Microsoft.AspNetCore.Mvc;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Models.Email;

namespace TaRge25Shop.Controllers
{
    public class EmailController : Controller
    {
        //konstruktor
        private readonly IEmailServices _emailServices;
        public EmailController
            (
                IEmailServices emailServices
            )
        {
            _emailServices = emailServices;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new EmailDto());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendEmail(EmailVM vm)
        {
            var files = Request.Form.Files.Any() ? Request.Form.Files.ToList() : new List<IFormFile>();

            var dto = new EmailDto
            {
                To = vm.To,
                Subject = vm.Subject,
                Body = vm.Body,
                Attachments = files
            };
            _emailServices.SendEmail(dto);

            return RedirectToAction(nameof(Index));
        }
    }
}
