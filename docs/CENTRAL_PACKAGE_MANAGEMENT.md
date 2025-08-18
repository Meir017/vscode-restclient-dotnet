# Central Package Management Migration

This document outlines the migration to NuGet Central Package Management (CPM) for the RESTClient.NET solution.

## Overview

We have migrated from traditional per-project package version management to Central Package Management as described in [Microsoft's documentation](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management).

## Benefits

1. **Centralized Version Control**: All package versions are managed in a single `Directory.Packages.props` file
2. **Consistency**: Ensures all projects use the same versions of shared dependencies
3. **Simplified Maintenance**: Version updates only need to be made in one place
4. **Reduced Conflicts**: Eliminates version conflicts between projects
5. **Better Security**: Easier to track and update packages for security vulnerabilities

## File Structure

### Directory.Packages.props (New)

This file is located at the solution root and contains all package versions:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.8" />
    <!-- ... other packages ... -->
  </ItemGroup>
</Project>
```

### Project Files (Modified)

All `.csproj` files now reference packages without versions:

```xml
<!-- Before -->
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.8" />

<!-- After -->
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
```

## Package Inventory

The following packages are managed centrally:

### Core Dependencies

- Microsoft.Extensions.Logging.Abstractions (9.0.8)
- Newtonsoft.Json (13.0.3)
- System.Text.RegularExpressions (4.3.1)

### ASP.NET Core Dependencies

- Microsoft.AspNetCore.Mvc.Testing (9.0.8)
- Microsoft.AspNetCore.OpenApi (9.0.8)
- Microsoft.Extensions.Hosting (9.0.8)
- Microsoft.Extensions.Logging (9.0.8)

### Entity Framework Dependencies

- Microsoft.EntityFrameworkCore.Design (9.0.8)
- Microsoft.EntityFrameworkCore.InMemory (9.0.8)
- Microsoft.EntityFrameworkCore.Sqlite (9.0.8)

### Testing Dependencies

- Microsoft.NET.Test.Sdk (17.14.1)
- xunit (2.9.3)
- xunit.core (2.9.3)
- xunit.extensibility.core (2.9.3)
- xunit.runner.visualstudio (3.1.4)
- coverlet.collector (6.0.4)
- AwesomeAssertions (9.1.0)

### Logging Dependencies

- Serilog.AspNetCore (9.0.0)
- Serilog.Sinks.Console (6.0.0)

### API Documentation

- Swashbuckle.AspNetCore (9.0.3)

## Usage

### Adding New Packages

1. Add the package version to `Directory.Packages.props`:

   ```xml
   <PackageVersion Include="MyNewPackage" Version="1.0.0" />
   ```

2. Reference the package in project files without version:

   ```xml
   <PackageReference Include="MyNewPackage" />
   ```

### Updating Package Versions

1. Update the version in `Directory.Packages.props`
2. The change applies to all projects referencing that package

### Per-Project Version Overrides

If needed, individual projects can override central versions:

```xml
<PackageReference Include="MyPackage" VersionOverride="2.0.0" />
```

## Validation

The migration has been validated with:

- ✅ Clean build of all projects
- ✅ Package restoration
- ✅ Full test suite execution (157 tests passed)
- ✅ Multi-targeting (.NET 9.0 and .NET Standard 2.0)

## Migration Impact

### Breaking Changes

- None. This is purely an internal build system change

### Behavioral Changes

- None. All functionality remains identical

### Build Process

- Build times may be slightly faster due to reduced package resolution complexity
- NuGet restore operations are more consistent

## Troubleshooting

### Common Issues

1. **Version conflicts**: Check `Directory.Packages.props` for duplicate entries
2. **Missing packages**: Ensure package is defined in `Directory.Packages.props`
3. **Override not working**: Use `VersionOverride` instead of `Version` attribute

### Verification Commands

```bash
# Verify CPM is enabled
dotnet build --verbosity diagnostic | grep -i "central"

# Check package versions
dotnet list package

# Validate all projects build
dotnet build vscode-restclient-dotnet.slnx
```

## References

- [Microsoft Docs: Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management)
- [NuGet Blog: Central Package Management](https://devblogs.microsoft.com/nuget/introducing-central-package-management/)
