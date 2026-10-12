using Microsoft.EntityFrameworkCore;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Data;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.Domain;
using Xunit;

namespace TaRge25Shop.Testing
{
    public class KindergardenTest : TestBase
    {
        [Fact]
        // Loob lasteaia, kui andmed on kehtivad
        public async Task Should_CreateKindergarden_WhenDataIsValid()
        {
            // Ülesseade
            var dto = MockKindergardenData();
            // Tegevus
            var result = await Svc<IKindergardenServices>().Create(dto);
            // Kontroll: kontrollime ka tegelikult salvestatud andmeid
            Assert.NotNull(result);
            Assert.NotNull(result.Id);
            Assert.NotEqual(Guid.Empty, result.Id.Value);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var saved = await Svc<IKindergardenServices>().DetailAsync(result.Id.Value);
            Assert.NotNull(saved);
            Assert.Equal(dto.KindergardenName, saved.KindergardenName);
            Assert.Equal(dto.GroupName, saved.GroupName);
            Assert.Equal(dto.TeacherName, saved.TeacherName);
            Assert.Equal(dto.ChildrenCount, saved.ChildrenCount);
        }

        [Fact]
        // Ei tohiks leida lasteaeda, kui ID on vale
        public async Task ShouldNot_GetKindergardenById_WhenDifferent()
        {
            var created = await CreateKindergarden();
            var result = await Svc<IKindergardenServices>().DetailAsync(Guid.NewGuid());

            Assert.Null(result);
            Assert.NotNull(await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value));
        }

        [Fact]
        // Peaks leidma lasteaia, kui ID on õige
        public async Task Should_GetKindergardenById_WhenIdIsEqual()
        {
            var created = await CreateKindergarden();
            var result = await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value);

            Assert.NotNull(result);
            Assert.Equal(created.Id, result.Id);
            Assert.Equal(created.KindergardenName, result.KindergardenName);
        }

        [Fact]
        // Peaks kustutama lasteaia andmebaasist, kui see on kustutatud
        public async Task Should_RemoveKindergardenFromDatabase_WhenKindergardenIsDeleted()
        {
            var created = await CreateKindergarden();
            var deleted = await Svc<IKindergardenServices>().Delete(created.Id!.Value);

            Assert.NotNull(deleted);
            Assert.Equal(created.Id, deleted.Id);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            Assert.Null(await Svc<IKindergardenServices>().DetailAsync(created.Id.Value));
        }

        [Fact]
        // Ei tohiks kustutada teist lasteaeda, kui üks on kustutatud
        public async Task ShouldNot_RemoveOtherKindergarden_WhenKindergardenIsDeleted()
        {
            var first = await CreateKindergarden();
            var second = await CreateKindergarden();
            var deleted = await Svc<IKindergardenServices>().Delete(second.Id!.Value);

            Assert.NotNull(deleted);
            Assert.Equal(second.Id, deleted.Id);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var remaining = await Svc<IKindergardenServices>().DetailAsync(first.Id!.Value);
            Assert.NotNull(remaining);
            Assert.Equal(first.Id, remaining.Id);
            Assert.Null(await Svc<IKindergardenServices>().DetailAsync(second.Id.Value));
        }

        [Fact]
        // Ei tohiks kustutada lasteaeda, kui ID ei ole olemas
        public async Task ShouldNot_DeleteKindergarden_WhenIdDoesNotExist()
        {
            var created = await CreateKindergarden();
            Assert.Null(await Svc<IKindergardenServices>().Delete(Guid.NewGuid()));
            Assert.NotNull(await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        // Peaks uuendama lasteaeda, kui andmed muutuvad
        public async Task Should_UpdateKindergarden_WhenDataChanges(bool clearTracker)
        {
            var created = await CreateKindergarden();
            var originalCreatedAt = created.CreatedAt;
            
            created.UpdatedAt = DateTime.Today.AddDays(-1);
            await Svc<TaRge25ShopContext>().SaveChangesAsync();
            var originalUpdatedAt = created.UpdatedAt;
            if (clearTracker)
            {
                Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            }

            var dto = MockKindergardenData();
            dto.Id = created.Id;
            dto.KindergardenName = "Updated kindergarden";
            dto.GroupName = "Tiigrid";
            dto.ChildrenCount = 12;
            dto.TeacherName = "Adolf";
            dto.CreatedAt = DateTime.MinValue;
            var updated = await Svc<IKindergardenServices>().Update(dto);

            Assert.NotNull(updated);
            Assert.Equal(created.Id, updated.Id);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var saved = await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value);
            Assert.NotNull(saved);
            Assert.Equal(dto.KindergardenName, saved.KindergardenName);
            Assert.Equal(dto.GroupName, saved.GroupName);
            Assert.Equal(dto.ChildrenCount, saved.ChildrenCount);
            Assert.Equal(dto.TeacherName, saved.TeacherName);
            Assert.Equal(originalCreatedAt, saved.CreatedAt);
            Assert.True(saved.UpdatedAt > originalUpdatedAt);
            Assert.Equal(1, await Svc<TaRge25ShopContext>().Kindergardens.CountAsync());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        // Ei tohiks uuendada lasteaeda, kui ID puudub
        public async Task ShouldNot_UpdateKindergarden_WhenIdIsMissing(bool useEmptyGuid)
        {
            var created = await CreateKindergarden();
            var dto = MockKindergardenData();
            dto.Id = useEmptyGuid ? Guid.Empty : null;
            dto.KindergardenName = "Must not be saved";

            Assert.Null(await Svc<IKindergardenServices>().Update(dto));
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var saved = await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value);
            Assert.NotNull(saved);
            Assert.Equal(created.KindergardenName, saved.KindergardenName);
            Assert.Equal(1, await Svc<TaRge25ShopContext>().Kindergardens.CountAsync());
        }

        [Fact]
        // Ei tohiks uuendada lasteaeda, kui ID ei eksisteeri
        public async Task ShouldNot_UpdateKindergarden_WhenIdDoesNotExist()
        {
            var dto = MockKindergardenData();
            dto.Id = Guid.NewGuid();

            Assert.Null(await Svc<IKindergardenServices>().Update(dto));
            Assert.Empty(await Svc<TaRge25ShopContext>().Kindergardens.ToListAsync());
        }

        [Theory]
        [InlineData(-999)]
        [InlineData(-1)]
        [InlineData(0)]
        // Peaks looma lasteaia, kui laste arv ei ole positiivne
        public async Task Should_CreateKindergarden_WhenChildrenCountIsNotPositive(int childrenCount)
        {
            var dto = MockKindergardenData();
            dto.ChildrenCount = childrenCount;

            var result = await Svc<IKindergardenServices>().Create(dto);

            Assert.NotNull(result);
            Assert.NotNull(result.Id);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var saved = Assert.Single(await Svc<TaRge25ShopContext>().Kindergardens.ToListAsync());
            Assert.Equal(result.Id, saved.Id);
            Assert.Equal(childrenCount, saved.ChildrenCount);
        }

        [Fact]
        // Peaks näitama kõiki lasteaedu, kui need on olemas andmebaasis
        public async Task ShouldShow_KindergardensWhenThereAreAnyInTheDatabase()
        {
            // Ülesseade
            var first = await CreateKindergarden();
            var second = await CreateKindergarden();
            var context = Svc<TaRge25ShopContext>();
            context.ChangeTracker.Clear();

            // Tegevus
            var results = await context.Kindergardens.ToListAsync();

            // Kontroll
            Assert.Equal(2, results.Count);
            Assert.Contains(results, x => x.Id == first.Id);
            Assert.Contains(results, x => x.Id == second.Id);
        }

        [Fact]
        // Peaks looma lasteaia, kui laste arv on null
        public async Task Should_CreateKindergarden_WhenChildrenCountIsZero()
        {
            var dto = MockKindergardenData();
            dto.ChildrenCount = 0;

            var result = await Svc<IKindergardenServices>().Create(dto);

            Assert.NotNull(result);
            Assert.NotNull(result.Id);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var saved = Assert.Single(await Svc<TaRge25ShopContext>().Kindergardens.ToListAsync());
            Assert.Equal(result.Id, saved.Id);
            Assert.Equal(0, saved.ChildrenCount);
        }

        [Fact]
        // Peaks looma lasteaia, kui laste arv on 100
        public async Task Should_CreateKindergarden_WhenChildrenCountIsHundred()
        {
            // Ülesseade
            var dto = MockKindergardenData();
            dto.ChildrenCount = 100;

            // Tegevus
            var result = await Svc<IKindergardenServices>().Create(dto);

            // Kontroll
            Assert.NotNull(result);
            Assert.NotNull(result.Id);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var saved = Assert.Single(await Svc<TaRge25ShopContext>().Kindergardens.ToListAsync());
            Assert.Equal(result.Id, saved.Id);
            Assert.Equal(100, saved.ChildrenCount);
        }

        [Fact]
        // Ei tohiks näidata lasteaedu, kui andmebaas on tühi
        public async Task ShouldNot_ShowKindergardens_WhenDatabaseIsEmpty()
        {
            var results = await Svc<TaRge25ShopContext>().Kindergardens.ToListAsync();

            Assert.Empty(results);
        }

        [Fact]
        // Ei tohiks leida lasteaeda, kui ID on tühi
        public async Task ShouldNot_GetKindergardenById_WhenIdIsEmpty()
        {
            var created = await CreateKindergarden();
            var result = await Svc<IKindergardenServices>().DetailAsync(Guid.Empty);

            Assert.Null(result);
            Assert.NotNull(await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value));
        }

        [Fact]
        // Ei tohiks kustutada lasteaeda, kui ID on tühi
        public async Task ShouldNot_DeleteKindergarden_WhenIdIsEmpty()
        {
            var created = await CreateKindergarden();
            var result = await Svc<IKindergardenServices>().Delete(Guid.Empty);

            Assert.Null(result);
            Assert.NotNull(await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value));
        }

        [Fact]
        // Ei tohiks kustutada lasteaeda, kui see on juba kustutatud
        public async Task ShouldNot_DeleteKindergarden_WhenAlreadyDeleted()
        {
            var created = await CreateKindergarden();
            var deleted = await Svc<IKindergardenServices>().Delete(created.Id!.Value);

            var result = await Svc<IKindergardenServices>().Delete(created.Id.Value);

            Assert.NotNull(deleted);
            Assert.Null(result);
            Assert.Empty(await Svc<TaRge25ShopContext>().Kindergardens.ToListAsync());
        }

        [Fact]
        // Ei tohiks uuendada lasteaeda, kui see on juba kustutatud
        public async Task ShouldNot_UpdateKindergarden_WhenAlreadyDeleted()
        {
            var created = await CreateKindergarden();
            var deleted = await Svc<IKindergardenServices>().Delete(created.Id!.Value);
            Assert.NotNull(deleted);
            var dto = MockKindergardenData();
            dto.Id = created.Id;

            var result = await Svc<IKindergardenServices>().Update(dto);

            Assert.Null(result);
            Assert.Empty(await Svc<TaRge25ShopContext>().Kindergardens.ToListAsync());
        }

        [Fact]
        // Peaks looma erinevad ID-d, kui kaks lasteaeda on loodud 
        public async Task Should_CreateDifferentIds_WhenTwoKindergardensAreCreated()
        {
            var first = await CreateKindergarden();
            var second = await CreateKindergarden();

            Assert.NotEqual(Guid.Empty, first.Id!.Value);
            Assert.NotEqual(Guid.Empty, second.Id!.Value);
            Assert.NotEqual(first.Id, second.Id);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            Assert.Equal(2, await Svc<TaRge25ShopContext>().Kindergardens.CountAsync());
        }

        [Fact]
        // Peaks looma lasteaia, kui nimi juba eksisteerib
        public async Task Should_CreateKindergarden_WhenNameAlreadyExists()
        {
            var first = await CreateKindergarden();
            var dto = MockKindergardenData();
            dto.KindergardenName = first.KindergardenName;

            var result = await Svc<IKindergardenServices>().Create(dto);

            Assert.NotNull(result);
            Assert.NotEqual(first.Id, result.Id);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var results = await Svc<TaRge25ShopContext>().Kindergardens.ToListAsync();
            Assert.Equal(2, results.Count);
            Assert.All(results, x => Assert.Equal(dto.KindergardenName, x.KindergardenName));
        }

        [Fact]
        // Peaks looma uue ID, kui DTO-l on juba ID
        public async Task Should_CreateNewId_WhenDtoAlreadyHasId()
        {
            var dto = MockKindergardenData();
            dto.Id = Guid.NewGuid();

            var result = await Svc<IKindergardenServices>().Create(dto);

            Assert.NotNull(result);
            Assert.NotNull(result.Id);
            Assert.NotEqual(Guid.Empty, result.Id.Value);
            Assert.NotEqual(dto.Id, result.Id);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var saved = Assert.Single(await Svc<TaRge25ShopContext>().Kindergardens.ToListAsync());
            Assert.Equal(result.Id, saved.Id);
        }

        [Fact]
        // Peaks uuendama lasteaia nime, kui selle nimi muutub
        public async Task Should_UpdateKindergardenName_WhenNameChanges()
        {
            // Ülesseade
            var created = await CreateKindergarden();
            var dto = MockKindergardenData();
            dto.Id = created.Id;
            dto.KindergardenName = "Uus lasteaed";

            // Tegevus
            var result = await Svc<IKindergardenServices>().Update(dto);

            // Kontroll
            Assert.NotNull(result);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var saved = await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value);
            Assert.NotNull(saved);
            Assert.Equal(dto.KindergardenName, saved.KindergardenName);
        }

        [Fact]
        // Peaks uuendama rühma nime, kui selle nimi muutub
        public async Task Should_UpdateGroupName_WhenGroupNameChanges()
        {
            var created = await CreateKindergarden();
            var dto = MockKindergardenData();
            dto.Id = created.Id;
            dto.GroupName = "Mesilased";

            var result = await Svc<IKindergardenServices>().Update(dto);

            Assert.NotNull(result);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var saved = await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value);
            Assert.NotNull(saved);
            Assert.Equal(dto.GroupName, saved.GroupName);
        }

        [Fact]
        // Pekas uuendam õpetaja nime, kui see muutub
        public async Task Should_UpdateTeacherName_WhenTeacherNameChanges()
        {
            var created = await CreateKindergarden();
            var dto = MockKindergardenData();
            dto.Id = created.Id;
            dto.TeacherName = "Mari";

            var result = await Svc<IKindergardenServices>().Update(dto);

            Assert.NotNull(result);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var saved = await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value);
            Assert.NotNull(saved);
            Assert.Equal(dto.TeacherName, saved.TeacherName);
        }

        [Fact]
        // Peaks uuendama laste arvu, kui see muutub nulliks
        public async Task Should_UpdateKindergarden_WhenChildrenCountBecomesZero()
        {
            var created = await CreateKindergarden();
            var dto = MockKindergardenData();
            dto.Id = created.Id;
            dto.ChildrenCount = 0;

            var result = await Svc<IKindergardenServices>().Update(dto);

            Assert.NotNull(result);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var saved = await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value);
            Assert.NotNull(saved);
            Assert.Equal(0, saved.ChildrenCount);
        }

        [Fact]
        // Ei tohiks uuendada teist lasteaeda, kui üks on juba uuendatud
        public async Task ShouldNot_UpdateOtherKindergarden_WhenOneKindergardenIsUpdated()
        {
            // Ülesseade
            var first = await CreateKindergarden();
            var second = await CreateKindergarden();
            var dto = MockKindergardenData();
            dto.Id = second.Id;
            dto.KindergardenName = "Uus lasteaed";
            dto.GroupName = "Mesilased";
            dto.TeacherName = "Mari";
            dto.ChildrenCount = 10;

            // Tegevus
            var result = await Svc<IKindergardenServices>().Update(dto);

            // Kontroll
            Assert.NotNull(result);
            Svc<TaRge25ShopContext>().ChangeTracker.Clear();
            var saved = await Svc<IKindergardenServices>().DetailAsync(first.Id!.Value);
            Assert.NotNull(saved);
            Assert.Equal(first.KindergardenName, saved.KindergardenName);
            Assert.Equal(first.GroupName, saved.GroupName);
            Assert.Equal(first.TeacherName, saved.TeacherName);
            Assert.Equal(first.ChildrenCount, saved.ChildrenCount);
            var updated = await Svc<IKindergardenServices>().DetailAsync(second.Id!.Value);
            Assert.NotNull(updated);
            Assert.Equal(dto.KindergardenName, updated.KindergardenName);
            Assert.Equal(dto.GroupName, updated.GroupName);
            Assert.Equal(dto.TeacherName, updated.TeacherName);
            Assert.Equal(dto.ChildrenCount, updated.ChildrenCount);
        }

        private async Task<Kindergarden> CreateKindergarden()
        {
            var dto = MockKindergardenData();
            var result = await Svc<IKindergardenServices>().Create(dto);
            Assert.NotNull(result);
            Assert.NotNull(result.Id);
            return result;
        }

        private static KindergardenDto MockKindergardenData() => new()
        {
            KindergardenName = "Test Kindergarten",
            ChildrenCount = 66,
            GroupName = "Test Group",
            TeacherName = "Test Teacher",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
    }
}
