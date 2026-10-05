using System;
using System.Collections.Generic;
using System.Text;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.ServiceInterface;
using Xunit;

namespace TaRge25Shop.Testing
{
    public class KindergardenTest : TestBase
    {
        [Fact]
        // See test kontrollib, et kui me loome uue lasteaia, siis see ei ole tühi ja tagastab tulemuse.
        // Sissejuhatuseks
        public async Task ShouldNot_AddEmptyKindergarden_WhenResultIsReturned()
        {
            // Ülesseade
            KindergardenDto dto = new KindergardenDto()
            {
                GroupName = "Mesimummud",
                ChildrenCount = 17,
                TeacherName = "Kallas",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            // Tegutsemine
            var result = await Svc<IKindergardenServices>().Create(dto);

            // Kontroll
            Assert.NotNull(result);
        }

        [Fact]
        // Test kontrollib, et kui me loome uue lasteaia, siis grupinimi ei saa olla number, vaid peab olema tekst.
        public async Task ShouldNot_AddKindergarden_WhenGroupNameIsNumber()
        {
            // Ülesseade
            KindergardenDto dto = new KindergardenDto()
            {
                GroupName = "123",
                ChildrenCount = 17,
                TeacherName = "Kallas",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            // Tegutsemine
            var result = await Svc<IKindergardenServices>().Create(dto);

            // Kontroll
            Assert.Null(result);
        }

        [Fact]
        // Test kontrollib, et lisatakse uus Id, kui me loome uue lasteaia.
        public async Task Should_AddNewId_WhenKindergardenIsCreated()
        {
            // Ülesseade
            KindergardenDto dto = new KindergardenDto()
            {
                GroupName = "Mesimummud",
                ChildrenCount = 17,
                TeacherName = "Kallas",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            // Tegutsemine
            var result = await Svc<IKindergardenServices>().Create(dto);
            // Kontroll
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
        }
        [Fact]
        // Test kontrollib, et kui me loome uue lasteaia, siis lapsed ei saa olla negatiivne arv.
        public async Task ShouldNot_AddKindergarden_WhenChildrenCountIsNegative()
        {
            // Ülesseade
            KindergardenDto dto = new KindergardenDto()
            {
                GroupName = "Mesimummud",
                ChildrenCount = -5,
                TeacherName = "Kallas",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            // Tegutsemine
            var result = await Svc<IKindergardenServices>().Create(dto);
            // Kontroll
            Assert.Null(result);
        }

        [Fact]
        // Test kontrollib, et laste arv ei saa olla liiga suur, näiteks üle 100.
        public async Task ShouldNot_AddKindergarden_WhenChildrenCountIsTooLarge()
        {
            // Ülesseade
            KindergardenDto dto = new KindergardenDto()
            {
                GroupName = "Mesimummud",
                ChildrenCount = 100,
                TeacherName = "Kallas",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            // Tegutsemine
            var result = await Svc<IKindergardenServices>().Create(dto);
            // Kontroll
            Assert.Null(result);
        }

        [Fact]
        // Peaks tagastama lasteaia detailid, kui me otsime olemasolevat Id-d.
        public async Task Should_ReturnKindergardenDetails_WhenIdExists()
        {
            // Ülesseade
            KindergardenDto dto = new KindergardenDto()
            {
                GroupName = "Mesimummud",
                ChildrenCount = 17,
                TeacherName = "Kallas",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            var createdKindergarden = await Svc<IKindergardenServices>().Create(dto);
            Assert.NotNull(createdKindergarden);
            // Tegutsemine
            var kindergardenId = Assert.IsType<Guid>(createdKindergarden.Id);
            var result = await Svc<IKindergardenServices>().DetailAsync(kindergardenId);
            // Kontroll
            Assert.NotNull(result);
            Assert.Equal(createdKindergarden.Id, result.Id);
        }

        [Fact]
        // Peaks tagastama null, kui me otsime mitteolemasolevat Id-d.
        public async Task Should_ReturnNull_WhenIdDoesNotExist()
        {
            // Tegutsemine
            var result = await Svc<IKindergardenServices>().DetailAsync(Guid.NewGuid());
            // Kontroll
            Assert.Null(result);
        }

        [Fact]
        // Peaks jätma muutmata loomis kuupäeva, kui me uuendame lasteaia andmeid.
        public async Task Should_NotChangeCreatedAt_WhenKindergardenIsUpdated()
        {
            // Ülesseade
            KindergardenDto dto = new KindergardenDto()
            {
                GroupName = "Mesimummud",
                ChildrenCount = 17,
                TeacherName = "Kallas",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            var createdKindergarden = await Svc<IKindergardenServices>().Create(dto);
            Assert.NotNull(createdKindergarden);
            var originalCreatedAt = createdKindergarden.CreatedAt;
            // Tegutsemine
            dto.Id = createdKindergarden.Id;
            dto.GroupName = "Uus Grupp";
            dto.CreatedAt = createdKindergarden.CreatedAt;
            var updatedKindergarden = await Svc<IKindergardenServices>().Update(dto);
            // Kontroll
            Assert.NotNull(updatedKindergarden);
            Assert.Equal(originalCreatedAt, updatedKindergarden.CreatedAt);
        }

        [Fact]
        // Peaks uuendama lasteaia andmeid, kui me muudame andmeid ja need andmed on kehtivad.
        public async Task Should_UpdateKindergarden_WhenDataIsValid()
        {
            // Ülesseade
            KindergardenDto dto = new KindergardenDto()
            {
                GroupName = "Mesimummud",
                ChildrenCount = 17,
                TeacherName = "Kallas",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            var createdKindergarden = await Svc<IKindergardenServices>().Create(dto);
            Assert.NotNull(createdKindergarden);
            // Tegutsemine
            dto.Id = createdKindergarden.Id;
            dto.GroupName = "Uus Grupp";
            dto.CreatedAt = createdKindergarden.CreatedAt;
            var updatedKindergarden = await Svc<IKindergardenServices>().Update(dto);
            // Kontroll
            Assert.NotNull(updatedKindergarden);
            Assert.Equal("Uus Grupp", updatedKindergarden.GroupName);
        }

        [Fact]
        // Ei tohiks uuendada lasteaia andmeid, kui Id ei eksisteeri.
        public async Task ShouldNot_UpdateKindergarden_WhenIdDoesNotExist()
        {
            // Ülesseade
            KindergardenDto dto = new KindergardenDto()
            {
                Id = Guid.NewGuid(),
                GroupName = "Mesimummud",
                ChildrenCount = 17,
                TeacherName = "Kallas",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            // Tegutsemine
            var result = await Svc<IKindergardenServices>().Update(dto);
            // Kontroll
            Assert.Null(result);
        }

        [Fact]
        // Peaks kustutama lasteaia, kui Id eksisteerib.
        public async Task Should_DeleteKindergarden_WhenIdExists()
        {
            // Ülesseade
            KindergardenDto dto = new KindergardenDto()
            {
                GroupName = "Mesimummud",
                ChildrenCount = 17,
                TeacherName = "Kallas",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            var createdKindergarden = await Svc<IKindergardenServices>().Create(dto);
            Assert.NotNull(createdKindergarden);
            // Tegutsemine
            var kindergardenId = Assert.IsType<Guid>(createdKindergarden.Id);
            var result = await Svc<IKindergardenServices>().Delete(kindergardenId);
            // Kontroll
            Assert.NotNull(result);
            Assert.Equal(kindergardenId, result.Id);
            Assert.Null(await Svc<IKindergardenServices>().DetailAsync(kindergardenId));
        }
    }
}
