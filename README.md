# TaRge25Progemine

## Selenium testing

`TaRge25Shop.SeleniumTesting` contains the spaceship frontend test imported from
`TARge25Testimine/TARge25Shop/TARge25Shop.SeleniumTesting` and adapted to this
project's namespaces and existing form selectors. It is included in
`TaRge25Shop/TaRge25Shop.slnx`.

On Windows, install the .NET 10 SDK and Firefox, then run from the repository root:

```powershell
dotnet test .\TaRge25Shop.SeleniumTesting\TaRge25Shop.SeleniumTesting.csproj
```

The test starts the application on a dynamically assigned local HTTP port, opens
Firefox headlessly using the copied `geckodriver.exe`, creates a spaceship, and
checks its name, type, and crew in the list. Each test fixture uses a separate
in-memory database, so SQL Server and a separately running application are not
required. The bundled driver is for Windows; other operating systems require a
matching geckodriver binary and a corresponding project configuration change.

## Spaceship service tests

`TaRge25Shop.Testing` checks create, lookup, update, delete, and invalid input
against a separate in-memory database for each test. Run these tests without a
browser or SQL Server:

```powershell
dotnet test .\TaRge25Shop.Testing\TaRge25Shop.Testing.csproj
```

Spaceships require positive engine power and more than three crew members.
Invalid create/update requests return no result and leave the database unchanged.
Updates require an existing spaceship ID and preserve the original creation date.
The web form displays validation errors for invalid input.

Run both the service tests and the Firefox test from the solution:

```powershell
dotnet test .\TaRge25Shop\TaRge25Shop.slnx
```
