using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TaRge25Shop.ApplicationServices.Services;
using TaRge25Shop.Controllers;
using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Data;
using TaRge25Shop.Models.Kindergarden;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    Console.WriteLine("PASS: " + message);
}

static IFormFile Picture(string name, params byte[] bytes) =>
    new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Files", name);

await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
var options = new DbContextOptionsBuilder<TaRge25ShopContext>().UseSqlite(connection).Options;
await using var context = new TaRge25ShopContext(options);
await context.Database.EnsureCreatedAsync();
var files = new FileServices(null!, context);
var service = new KindergardenServices(context, files);
var controller = new KindergardenController(service, context);

var create = await controller.Create(new KindergardenCreateUpdateViewModel
{
    GroupName = "Butterflies", ChildrenCount = 12,
    KindergardenName = "Garden", TeacherName = "Teacher",
    Files = [Picture("one.png", 1, 2, 3), Picture("two.JPG", 4, 5), Picture("empty.png")]
});
Check(create is RedirectToActionResult { ActionName: "Index" }, "Create redirects to the list");
var garden = await context.Kindergardens.SingleAsync();
var id = garden.Id!.Value;
var createdAt = garden.CreatedAt;
var images = await context.FileToDatabases.Where(x => x.KindergardenId == id).ToListAsync();
Check(images.Count == 2 && images.All(x => x.RealEstateId == null), "Multiple pictures are saved to the kindergarten; empty files are skipped");
var first = images.Single(x => x.ImageTitle == "one.png");
var image = (FileContentResult)await controller.GetImage(first.Id);
Check(image.ContentType == "image/png" && image.FileContents.SequenceEqual(new byte[] { 1, 2, 3 }), "Picture endpoint returns the original bytes and content type");
var jpg = images.Single(x => x.ImageTitle == "two.JPG");
Check(((FileContentResult)await controller.GetImage(jpg.Id)).ContentType == "image/jpeg", "Uppercase JPG extension is served correctly");

context.ChangeTracker.Clear();
var update = await controller.Update(new KindergardenCreateUpdateViewModel
{
    Id = id, GroupName = "Updated", ChildrenCount = 15,
    KindergardenName = "Garden", TeacherName = "Teacher",
    CreatedAt = DateTime.MinValue, Files = [Picture("three.webp", 6, 7)]
});
Check(update is RedirectToActionResult, "Update accepts additional uploads");
garden = await context.Kindergardens.SingleAsync();
Check(garden.CreatedAt == createdAt && garden.ChildrenCount == 15, "Update preserves creation time and updates record fields");
Check(await context.FileToDatabases.CountAsync(x => x.KindergardenId == id) == 3, "Adding a picture keeps existing pictures");
var editView = (ViewResult)await controller.Update(id);
Check(((KindergardenCreateUpdateViewModel)editView.Model!).Images.Count == 3, "Edit view displays all pictures");
var detailView = (ViewResult)await controller.Details(id);
Check(((KindergardenDetailViewModel)detailView.Model!).Images.Count == 3, "Details view displays all pictures");
var deleteView = (ViewResult)await controller.Delete(id);
Check(((KindergardenDeleteViewModel)deleteView.Model!).Images.Count == 3, "Delete confirmation displays all pictures");

controller.ModelState.AddModelError("ChildrenCount", "Invalid value");
var invalidView = (ViewResult)await controller.Update(new KindergardenCreateUpdateViewModel { Id = id });
Check(((KindergardenCreateUpdateViewModel)invalidView.Model!).Images.Count == 3, "Invalid update redisplays existing pictures");
controller.ModelState.Clear();

var remove = (RedirectToActionResult)await controller.RemoveImage(first.Id);
Check(remove.ActionName == "Update" && Equals(remove.RouteValues!["id"], id), "Picture removal returns to the owning kindergarten");
Check(await context.FileToDatabases.CountAsync(x => x.KindergardenId == id) == 2, "Only the selected picture is removed");
Check(await controller.GetImage(first.Id) is NotFoundResult, "Deleted picture returns 404");
var realEstate = await new RealEstateServices(context, files).Create(new RealEstateDto
{
    Area = 100, Location = "Town", BuildingType = "House", RoomNumber = 3,
    Files = [Picture("house.png", 9, 10)]
});
var estateImage = await context.FileToDatabases.SingleAsync(x => x.RealEstateId == realEstate.Id);
Check(await controller.RemoveImage(estateImage.Id) is NotFoundResult, "Kindergarten cannot remove real-estate pictures");
Check(await controller.GetImage(estateImage.Id) is NotFoundResult, "Kindergarten picture endpoint excludes real-estate pictures");
Check(await controller.RemoveImage(Guid.NewGuid()) is NotFoundResult, "Unknown picture returns 404");

var otherGarden = await service.Create(new KindergardenDto { GroupName = "Other", Files = [Picture("other.png", 8)] });
await controller.DeleteConfirmation(id);
Check(!await context.Kindergardens.AnyAsync(x => x.Id == id)
    && !await context.FileToDatabases.AnyAsync(x => x.KindergardenId == id), "Deleting a kindergarten removes its pictures");
Check(await context.FileToDatabases.AnyAsync(x => x.KindergardenId == otherGarden.Id)
    && await context.FileToDatabases.AnyAsync(x => x.RealEstateId == realEstate.Id), "Deleting a kindergarten preserves other records and real-estate pictures");
Check(await service.Delete(Guid.NewGuid()) == null, "Deleting a missing record is handled");
Check(await service.Update(new KindergardenDto { Id = Guid.NewGuid(), Files = [Picture("missing.png", 1)] }) == null,
    "Updating a missing record does not create orphaned uploads");
Check(await controller.Update(Guid.NewGuid()) is NotFoundResult
    && await controller.Details(Guid.NewGuid()) is NotFoundResult
    && await controller.Delete(Guid.NewGuid()) is NotFoundResult, "Missing record pages return 404");

foreach (var action in new[] { "Create", "Update", "RemoveImage", "DeleteConfirmation" })
{
    var post = typeof(KindergardenController).GetMethods().Single(x => x.Name == action && x.GetCustomAttribute<HttpPostAttribute>() != null);
    Check(post.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() != null, action + " POST requires an antiforgery token");
}

// SQL Server model and migration checks do not require a running SQL Server.
await using var sqlContext = new TaRge25ShopContext(new DbContextOptionsBuilder<TaRge25ShopContext>()
    .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=KindergardenImageTests;Trusted_Connection=True").Options);
Check(!sqlContext.Database.HasPendingModelChanges(), "Merged SQL Server migration snapshot matches the runtime model");
var script = sqlContext.GetService<IMigrator>().GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);
Check(script.Contains("CREATE TABLE [Kindergardens]") && script.Contains("CREATE TABLE [RealEstates]")
    && script.Contains("ALTER TABLE [FileToDatabases] ADD [KindergardenId] uniqueidentifier NULL"),
    "Migration script supports both features and adds the nullable kindergarten image column");
Console.WriteLine("All kindergarten picture checks passed.");
