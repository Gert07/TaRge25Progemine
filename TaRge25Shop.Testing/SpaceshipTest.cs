using Microsoft.EntityFrameworkCore;
using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Data;
using Xunit;

namespace TaRge25Shop.Testing;

public class SpaceshipTest : TestBase
{
    [Fact]
    public async Task Should_CreateSpaceship_WhenDataIsValid()
    {
        // Ülesseade
        var dto = MockSpaceshipData();

        // Tegevus
        var result = await Svc<ISpaceshipServices>().Create(dto);

        // Kontroll: kontrollime ka tegelikult salvestatud andmeid.
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotEqual(Guid.Empty, result.Id.Value);
        Svc<TaRge25ShopContext>().ChangeTracker.Clear();
        var saved = await Svc<ISpaceshipServices>().DetailAsync(result.Id.Value);
        Assert.NotNull(saved);
        Assert.Equal(dto.Name, saved.Name);
        Assert.Equal(dto.ShipType, saved.ShipType);
        Assert.Equal(dto.Crew, saved.Crew);
        Assert.Equal(dto.EnginePower, saved.EnginePower);
    }

    [Fact]
    public async Task ShouldNot_GetSpaceshipById_WhenIdIsDifferent()
    {
        var created = await CreateSpaceship();
        var result = await Svc<ISpaceshipServices>().DetailAsync(Guid.NewGuid());

        Assert.Null(result);
        Assert.NotNull(await Svc<ISpaceshipServices>().DetailAsync(created.Id!.Value));
    }

    [Fact]
    public async Task Should_GetSpaceshipById_WhenIdIsEqual()
    {
        var created = await CreateSpaceship();
        var result = await Svc<ISpaceshipServices>().DetailAsync(created.Id!.Value);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.Name, result.Name);
    }

    [Fact]
    public async Task Should_RemoveSpaceshipFromDatabase_WhenSpaceshipIsDeleted()
    {
        var created = await CreateSpaceship();
        var deleted = await Svc<ISpaceshipServices>().Delete(created.Id!.Value);

        Assert.NotNull(deleted);
        Assert.Equal(created.Id, deleted.Id);
        Svc<TaRge25ShopContext>().ChangeTracker.Clear();
        Assert.Null(await Svc<ISpaceshipServices>().DetailAsync(created.Id.Value));
    }

    [Fact]
    public async Task ShouldNot_RemoveOtherSpaceship_WhenSpaceshipIsDeleted()
    {
        var first = await CreateSpaceship();
        var second = await CreateSpaceship();
        var deleted = await Svc<ISpaceshipServices>().Delete(second.Id!.Value);

        Assert.NotNull(deleted);
        Assert.Equal(second.Id, deleted.Id);
        Svc<TaRge25ShopContext>().ChangeTracker.Clear();
        var remaining = await Svc<ISpaceshipServices>().DetailAsync(first.Id!.Value);
        Assert.NotNull(remaining);
        Assert.Equal(first.Id, remaining.Id);
        Assert.Null(await Svc<ISpaceshipServices>().DetailAsync(second.Id.Value));
    }

    [Fact]
    public async Task ShouldNot_DeleteSpaceship_WhenIdDoesNotExist()
    {
        var created = await CreateSpaceship();
        Assert.Null(await Svc<ISpaceshipServices>().Delete(Guid.NewGuid()));
        Assert.NotNull(await Svc<ISpaceshipServices>().DetailAsync(created.Id!.Value));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_UpdateSpaceship_WhenDataChanges(bool clearTracker)
    {
        var created = await CreateSpaceship();
        var originalCreatedAt = created.CreatedAt;
        // Kindel varasem aeg teeb UpdatedAt kontrolli sõltumatuks kella täpsusest.
        created.UpdatedAt = DateTime.Today.AddDays(-1);
        await Svc<TaRge25ShopContext>().SaveChangesAsync();
        var originalUpdatedAt = created.UpdatedAt;
        if (clearTracker)
        {
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
        }

        var dto = MockSpaceshipData();
        dto.Id = created.Id;
        dto.Name = "Updated spaceship";
        dto.ShipType = "Explorer";
        dto.Crew = 12;
        dto.EnginePower = 2500;
        dto.CreatedAt = DateTime.MinValue;
        var updated = await Svc<ISpaceshipServices>().Update(dto);

        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Svc<TaRge25ShopContext>().ChangeTracker.Clear();
        var saved = await Svc<ISpaceshipServices>().DetailAsync(created.Id!.Value);
        Assert.NotNull(saved);
        Assert.Equal(dto.Name, saved.Name);
        Assert.Equal(dto.ShipType, saved.ShipType);
        Assert.Equal(dto.Crew, saved.Crew);
        Assert.Equal(dto.EnginePower, saved.EnginePower);
        Assert.Equal(originalCreatedAt, saved.CreatedAt);
        Assert.True(saved.UpdatedAt > originalUpdatedAt);
        Assert.Equal(1, await Svc<TaRge25ShopContext>().Spaceships.CountAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ShouldNot_UpdateSpaceship_WhenIdIsMissing(bool useEmptyGuid)
    {
        var created = await CreateSpaceship();
        var dto = MockSpaceshipData();
        dto.Id = useEmptyGuid ? Guid.Empty : null;
        dto.Name = "Must not be saved";

        Assert.Null(await Svc<ISpaceshipServices>().Update(dto));
        Svc<TaRge25ShopContext>().ChangeTracker.Clear();
        var saved = await Svc<ISpaceshipServices>().DetailAsync(created.Id!.Value);
        Assert.NotNull(saved);
        Assert.Equal(created.Name, saved.Name);
        Assert.Equal(1, await Svc<TaRge25ShopContext>().Spaceships.CountAsync());
    }

    [Fact]
    public async Task ShouldNot_UpdateSpaceship_WhenIdDoesNotExist()
    {
        var dto = MockSpaceshipData();
        dto.Id = Guid.NewGuid();

        Assert.Null(await Svc<ISpaceshipServices>().Update(dto));
        Assert.Empty(await Svc<TaRge25ShopContext>().Spaceships.ToListAsync());
    }

    [Theory]
    [InlineData(-999)]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task ShouldNot_CreateSpaceship_WhenEnginePowerIsNotPositive(int enginePower)
    {
        var dto = MockSpaceshipData();
        dto.EnginePower = enginePower;

        Assert.Null(await Svc<ISpaceshipServices>().Create(dto));
        Assert.Empty(await Svc<TaRge25ShopContext>().Spaceships.ToListAsync());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task ShouldNot_CreateSpaceship_WhenCrewIsThreeOrLess(int crew)
    {
        var dto = MockSpaceshipData();
        dto.Crew = crew;

        Assert.Null(await Svc<ISpaceshipServices>().Create(dto));
        Assert.Empty(await Svc<TaRge25ShopContext>().Spaceships.ToListAsync());
    }

    [Fact]
    public async Task Should_CreateSpaceship_WhenCrewIsFourAndEnginePowerIsOne()
    {
        var dto = MockSpaceshipData();
        dto.Crew = 4;
        dto.EnginePower = 1;

        var result = await Svc<ISpaceshipServices>().Create(dto);
        Assert.NotNull(result);
        Assert.Equal(4, result.Crew);
        Assert.Equal(1, result.EnginePower);
    }

    [Theory]
    [InlineData(0, 12)]
    [InlineData(-1, 12)]
    [InlineData(100, 3)]
    [InlineData(100, 0)]
    public async Task ShouldNot_UpdateSpaceship_WhenDataIsInvalid(int enginePower, int crew)
    {
        var created = await CreateSpaceship();
        var originalPower = created.EnginePower;
        var originalCrew = created.Crew;
        var dto = MockSpaceshipData();
        dto.Id = created.Id;
        dto.EnginePower = enginePower;
        dto.Crew = crew;

        Assert.Null(await Svc<ISpaceshipServices>().Update(dto));
        Svc<TaRge25ShopContext>().ChangeTracker.Clear();
        var saved = await Svc<ISpaceshipServices>().DetailAsync(created.Id!.Value);
        Assert.NotNull(saved);
        Assert.Equal(originalPower, saved.EnginePower);
        Assert.Equal(originalCrew, saved.Crew);
    }

    private async Task<Spaceship> CreateSpaceship()
    {
        var result = await Svc<ISpaceshipServices>().Create(MockSpaceshipData());
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        return result;
    }

    private static SpaceshipDto MockSpaceshipData() => new()
    {
        Name = "RAKETT69",
        ShipType = "lendav taldrik",
        Crew = 67,
        EnginePower = 69,
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now
    };
}
