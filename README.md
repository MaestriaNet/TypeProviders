# Maestria Type Providers

[![Build status](https://ci.appveyor.com/api/projects/status/mvosd40vqsgrvkr0/branch/master?svg=true)](https://ci.appveyor.com/project/fabionaspolini/typeproviders/branch/master)
[![NuGet](https://buildstats.info/nuget/Maestria.TypeProviders)](https://www.nuget.org/packages/Maestria.TypeProviders)
[![MyGet](https://img.shields.io/myget/maestrianet/v/Maestria.TypeProviders?label=MyGet)](https://www.myget.org/feed/maestrianet/package/nuget/Maestria.TypeProviders)

[![Build History](https://buildstats.info/appveyor/chart/fabionaspolini/typeproviders?branch=master)](https://ci.appveyor.com/project/fabionaspolini/typeproviders/history?branch=master)

[![donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/donate?hosted_button_id=8RSES6GAYH9BL)

## What is Maestria Type Providers?

Source Generator pack to increase productivity and improve source code writing.

## How install and configure package?

First, install [Maestria Type Providers](https://www.nuget.org/packages/Maestria.TypeProviders/) from the dotnet cli command line:

```bash
dotnet add package Maestria.TypeProviders
```

And configure at `.csproj` file to emit generated files on hard disk:

```xml
<!-- Enable source disk file write to correct IDE's works -->
<PropertyGroup>
    <CompilerGeneratedFilesOutputPath>$(MSBuildProjectDirectory)/generated</CompilerGeneratedFilesOutputPath>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>

<!-- Remove files on build start to solve recreate bug message "alwaready exists" -->
<Target Name="ExcludeGenerated" BeforeTargets="AssignTargetPaths">
    <ItemGroup>
        <Generated Include="generated/**/*.cs" />
        <Compile Remove="@(Generated)" />
    </ItemGroup>
    <Delete Files="@(Generated)" />
</Target>
```

[Sample of .csproj file](samples/ExcelSample/ExcelSample.csproj#L7)

## Providers x Dependencies

This package does not include dependencies on build output or project, you need another install dependencies to generated source code use on you project, bellow instructions:

### Excel

Attribute: [ExcelProvider](src/Excel/ExcelProviderAttribute.cs)

- [Maestria.FluentCast](https://github.com/MaestriaNet/FluentCast)
- [ClosedXML](https://github.com/ClosedXML/ClosedXML)
- [Source Code use sample](samples/ExcelSample/Program.cs#L12)

```bash
dotnet add package Maestria.FluentCast
dotnet add package ClosedXML
```

```csharp
[ExcelProviderAttribute(TemplatePath = @"../../resources/Excel.xlsx")]
public partial class MyExcelData
{
}

var data = MyExcelDataFactory.Load(filePath);
foreach (var item in data)
  // Access strong typing by "data.<field-name>"
```

[![buy-me-a-coffee](resources/buy-me-a-coffee.png)](https://www.paypal.com/donate?hosted_button_id=8RSES6GAYH9BL)
[![smile.png](resources/smile.png)](https://www.paypal.com/donate?hosted_button_id=8RSES6GAYH9BL)

If my contributions helped you, please help me buy a coffee :D

[![donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/donate?hosted_button_id=8RSES6GAYH9BL)
