# NuGet Publishing Setup Guide

## 1. Repository Secrets Configuration

### Required GitHub Secrets
You need to add the following secret to your repository settings:

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**
3. Add:
   - **Name**: `NUGET_API_KEY`
   - **Value**: Your NuGet API key (get from https://www.nuget.org/account/apikeys)

### Getting a NuGet API Key
1. Visit https://www.nuget.org/account/apikeys
2. Create a new API key with:
   - **Package Owner**: Select your account
   - **Scopes**: Push new packages and package versions
   - **Select Packages**: Choose "All packages" or specific pattern
   - **Glob Pattern**: `RESTClient.NET.*` (if you want to restrict to your packages)

## 2. Release Process

### Manual Release Process
1. **Update version** in both project files:
   - `src/RESTClient.NET.Core/RESTClient.NET.Core.csproj`
   - `src/RESTClient.NET.Testing/RESTClient.NET.Testing.csproj`

2. **Commit and push** version changes:
   ```bash
   git add .
   git commit -m "chore: bump version to 1.0.0"
   git push origin main
   ```

3. **Create and push a tag**:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

4. **Monitor the workflow** at: https://github.com/Meir017/vscode-restclient-dotnet/actions

### Automated Release Process (Recommended)
Use the provided `scripts/release.ps1` script:

```powershell
# For a stable release
.\scripts\release.ps1 -Version "1.0.0" -UpdateProjects

# For a preview release  
.\scripts\release.ps1 -Version "1.0.1-preview.1" -UpdateProjects
```

## 3. Version Management

### Current Versioning Strategy
- **Stable releases**: `1.0.0`, `1.1.0`, `2.0.0`
- **Preview releases**: `1.0.0-preview.1`, `2.0.0-alpha.1`
- **Beta releases**: `1.0.0-beta.1`, `1.0.0-rc.1`

### Project File Version Properties
Both packages use these MSBuild properties:
- `PackageVersion`: Controls the NuGet package version
- `GeneratePackageOnBuild`: Automatically creates packages on build

## 4. Package Information

### RESTClient.NET.Core
- **PackageId**: `RESTClient.NET.Core`
- **Description**: Core library for parsing HTTP files
- **Target Frameworks**: .NET 9.0, .NET Standard 2.0

### RESTClient.NET.Testing
- **PackageId**: `RESTClient.NET.Testing`  
- **Description**: ASP.NET Core integration testing framework
- **Target Frameworks**: .NET 9.0

## 5. Workflow Features

### What the Release Workflow Does
1. ✅ Triggers on git tags starting with `v*`
2. ✅ Builds and tests the entire solution
3. ✅ Packs both NuGet packages with tag version
4. ✅ Publishes to NuGet.org
5. ✅ Creates a GitHub release
6. ✅ Uploads package artifacts
7. ✅ Detects preview/alpha/beta for prerelease marking

### Workflow Outputs
- **NuGet packages** published to https://www.nuget.org
- **GitHub release** created with package information
- **Build artifacts** available for download

## 6. Troubleshooting

### Common Issues
- **Authentication failure**: Check that `NUGET_API_KEY` secret is set correctly
- **Duplicate package**: Workflow uses `--skip-duplicate` to handle this gracefully
- **Build failures**: Ensure all tests pass locally before creating a release tag
- **Version conflicts**: Make sure project file versions match the git tag

### Monitoring
- **GitHub Actions**: Monitor workflow runs at `/actions`
- **NuGet.org**: Check package status at `/packages/manage`
- **Package downloads**: View stats on your NuGet package pages

## 7. Best Practices

### Before Releasing
1. ✅ Run full test suite locally: `dotnet test vscode-restclient-dotnet.slnx`
2. ✅ Update documentation and changelog
3. ✅ Test packages locally: `dotnet pack --configuration Release`
4. ✅ Verify version numbers in project files

### Release Cadence
- **Major versions**: Breaking changes, new architecture
- **Minor versions**: New features, backwards compatible
- **Patch versions**: Bug fixes, small improvements
- **Preview versions**: Early access, testing new features
