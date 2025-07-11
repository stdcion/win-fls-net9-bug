# FLS (Fiber Local Storage) Fails in .NET ≥ 9.0.5 on Windows x64

## Description

A compatibility issue occurs when using Fiber Local Storage (FLS) with .NET 9.0.5 and later on Windows x64 platforms.
The same code works correctly in .NET 9.0.4 and earlier versions (include .NET 8.0, .NET 7.0, .NET 6.0, 
.NET Framework 4.5, etc.)

## Prerequisite

- [.NET 9.0.4 (SDK 9.0.203)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-9.0.203-windows-x64-installer)

- [.NET 9.0.5 (SDK 9.0.300)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-9.0.300-windows-x64-installer)

## Steps to reproduce:

- Clone the repository:

```shell
git clone git@github.com:stdcion/win-fls-net9-bug.git
```

- Build the project in Release (or Debug, it doesn't matter):

```shell
cd win-fls-net9-bug
dotnet build --configuration Release
```

- Run the application with different .NET versions:

```shell
cd win-fls-net9-bug\bin\Release\net9.0\
dotnet --fx-version 9.0.4 .\win-fls-net9-bug.dll # works correctly
dotnet --fx-version 9.0.5 .\win-fls-net9-bug.dll # fails
```
