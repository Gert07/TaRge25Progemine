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
        public async Task Should_CreateKindergarden_WhenDataIsValid()
        {
            // Ülesseade
            var dto = MockKindergardenData();
            // Tegevus
            var result = await Svc<IKindergardenServices>().Create(dto);
            // Kontroll: kontrollime ka tegelikult salvestatud andmeid.
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
        public async Task ShouldNot_GetKindergardenById_WhenDifferent()
        {
            var created = await CreateKindergarden();
            var result = await Svc<IKindergardenServices>().DetailAsync(Guid.NewGuid());

            Assert.Null(result);
            Assert.NotNull(await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value));
        }

        [Fact]
        public async Task Should_GetKindergardenById_WhenIdIsEqual()
        {
            var created = await CreateKindergarden();
            var result = await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value);

            Assert.NotNull(result);
            Assert.Equal(created.Id, result.Id);
            Assert.Equal(created.KindergardenName, result.KindergardenName);
        }

        [Fact]
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
        public async Task ShouldNot_DeleteKindergarden_WhenIdDoesNotExist()
        {
            var created = await CreateKindergarden();
            Assert.Null(await Svc<IKindergardenServices>().Delete(Guid.NewGuid()));
            Assert.NotNull(await Svc<IKindergardenServices>().DetailAsync(created.Id!.Value));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Should_UpdateKindergarden_WhenDataChanges(bool clearTracker)
        {
            var created = await CreateKindergarden();
            var originalCreatedAt = created.CreatedAt;
            // Kindel varasem aeg teeb UpdatedAt kontrolli sõltumatuks kella täpsusest.
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
