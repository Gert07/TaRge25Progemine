using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Models.Cocktail;

namespace TaRge25Shop.Controllers
{
    public class CocktailController : Controller
    {
        private readonly ICocktailServices _cocktailServices;
        private readonly ILogger<CocktailController> _logger;

        public CocktailController(ICocktailServices cocktailServices, ILogger<CocktailController> logger)
        {
            _cocktailServices = cocktailServices;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new CocktailSearchViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SearchCocktail(CocktailSearchViewModel model)
        {
            ValidateName(model.CocktailName);
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            return RedirectToAction(nameof(Cocktail), new { cocktail = model.CocktailName.Trim() });
        }

        [HttpGet]
        public async Task<IActionResult> Cocktail(string? cocktail, CancellationToken cancellationToken = default)
        {
            var search = new CocktailSearchViewModel { CocktailName = cocktail?.Trim() ?? string.Empty };
            ValidateName(search.CocktailName);
            if (!ModelState.IsValid)
            {
                return View("Index", search);
            }

            try
            {
                var result = await _cocktailServices.SearchByNameAsync(search.CocktailName, cancellationToken);
                if (result == null)
                {
                    search.Message = $"Nimele \"{search.CocktailName}\" vastavaid kokteile ei leitud. Proovi teist nime.";
                    return View("Index", search);
                }

                return View(CocktailViewModel.FromDto(result));
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "Cocktail API request failed.");
            }
            catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(exception, "Cocktail API request timed out.");
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Cocktail API returned an invalid response.");
            }

            ModelState.AddModelError(string.Empty, "Kokteiliteenus pole praegu saadaval. Proovi veidi aja pärast uuesti.");
            return View("Index", search);
        }

        private void ValidateName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.TryAddModelError(nameof(CocktailSearchViewModel.CocktailName), "Sisesta kokteili nimi.");
            }
            else if (name.Length > CocktailSearchViewModel.MaxNameLength)
            {
                ModelState.TryAddModelError(nameof(CocktailSearchViewModel.CocktailName),
                    "Kokteili nimi võib olla kuni 100 märki pikk.");
            }
        }
    }
}
