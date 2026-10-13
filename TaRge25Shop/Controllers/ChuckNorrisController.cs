using Microsoft.AspNetCore.Mvc;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Models.ChuckNorris;


namespace TaRge25Shop.Controllers
{
    public class ChuckNorrisController : Controller
    {
        private readonly IChuckNorrisServices _chuckNorrisServices;

        public ChuckNorrisController
            (
                IChuckNorrisServices chuckNorrisServices
            )
        {
            _chuckNorrisServices = chuckNorrisServices;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var joke = await _chuckNorrisServices.ChuckNorrisResultHttpClient();

            ChuckNorrisVM vm = new();

            vm.Categories = string.Join(", ", joke.Categories ?? new List<string>());
            vm.CreatedAt = joke.CreatedAt;
            vm.IconUrl = joke.IconUrl;
            vm.Id = joke.Id;
            vm.UpdatedAt = joke.UpdatedAt;
            vm.Url = joke.Url;
            vm.Value = joke.Value;

            return View(vm);
        }

        [HttpGet]
        public IActionResult Joke()
        {
            return RedirectToAction(nameof(Index));
        }
    }
}
