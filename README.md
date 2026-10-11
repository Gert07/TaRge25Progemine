# TaRge25Progemine

Kindergarten records support multiple picture uploads when creating or updating a
record. Pictures appear on edit, details, and delete pages. Individual pictures
can be removed on the edit page; deleting a record also removes its pictures.
Kindergarten and real-estate pictures share `FileToDatabases`, with separate
nullable owner IDs.

Requires .NET 10 and SQL Server configured through the application's
`DefaultConnection` connection string. Before running the updated application,
apply the migrations with the .NET EF tool (version 10.0.11):

```powershell
dotnet ef database update --project TaRge25.Data --startup-project TaRge25Shop
dotnet run --project TaRge25Shop
```

The `KindergardenImages` migration adds a nullable `KindergardenId` column and
preserves existing real-estate pictures. The combined migration history supports
databases previously used by either branch.

Build and run the picture regression checks (SQLite in memory, no development
database changes):

```powershell
dotnet build TaRge25Shop/TaRge25Shop.slnx
dotnet run --project Testing/Kindergarden.ImageTests
```

