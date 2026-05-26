# MyApp

A .NET Core Web API skeleton project with a clean solution structure, a sample controller-based API, and an xUnit test project.

## Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) or later

## Project Structure

```
MyApp/
├── MyApp.sln
├── .gitignore
├── README.md
├── src/
│   └── MyApp.Api/          # ASP.NET Core Web API
│       ├── Controllers/
│       ├── Program.cs
│       ├── appsettings.json
│       └── appsettings.Development.json
└── tests/
    └── MyApp.Tests/         # xUnit test project
        ├── MyApp.Tests.csproj
        └── UnitTest1.cs
```

## Build

```bash
dotnet build
```

## Run

```bash
dotnet run --project src/MyApp.Api
```

The API will be available at `https://localhost:5001` (or the port configured in `Properties/launchSettings.json`).

## Test

```bash
dotnet test
```
