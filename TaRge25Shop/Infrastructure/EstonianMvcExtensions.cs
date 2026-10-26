using Microsoft.Extensions.DependencyInjection;

namespace TaRge25Shop.Infrastructure;

public static class EstonianMvcExtensions
{
    public static IMvcBuilder AddEstonianUi(this IMvcBuilder builder) => builder.AddMvcOptions(options =>
    {
        var messages = options.ModelBindingMessageProvider;
        messages.SetAttemptedValueIsInvalidAccessor((value, field) => $"Väärtus „{value}” ei sobi väljale „{field}”.");
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"Väärtus „{value}” ei sobi.");
        messages.SetUnknownValueIsInvalidAccessor(field => $"Väljale „{field}” sisestatud väärtus ei sobi.");
        messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Sisestatud väärtus ei sobi.");
        messages.SetValueIsInvalidAccessor(value => $"Väärtus „{value}” ei sobi.");
        messages.SetValueMustNotBeNullAccessor(_ => "See väli on kohustuslik.");
        messages.SetValueMustBeANumberAccessor(field => $"{field} peab olema arv.");
        messages.SetNonPropertyValueMustBeANumberAccessor(() => "Sisesta arv.");
        messages.SetMissingBindRequiredValueAccessor(field => $"Väli „{field}” on kohustuslik.");
        messages.SetMissingKeyOrValueAccessor(() => "Nõutav väärtus puudub.");
        messages.SetMissingRequestBodyRequiredValueAccessor(() => "Päringu sisu on kohustuslik.");
    });
}
