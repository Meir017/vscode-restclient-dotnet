#Requires -Version 5.1

<#
.SYNOPSIS
    Automates the release process for RESTClient.NET packages.

.DESCRIPTION
    This script automates the version updating, tagging, and release process for RESTClient.NET.
    It can update project files, create git tags, and trigger the GitHub Actions release workflow.

.PARAMETER Version
    The version to release (e.g., "1.0.0", "1.0.1-preview.1")

.PARAMETER UpdateProjects
    Switch to update project file versions before creating the release

.PARAMETER DryRun
    Switch to simulate the release process without making any changes

.PARAMETER Force
    Switch to force the release even if there are uncommitted changes

.EXAMPLE
    .\release.ps1 -Version "1.0.0" -UpdateProjects
    
    Updates project files to version 1.0.0 and creates a release tag.

.EXAMPLE
    .\release.ps1 -Version "1.0.1-preview.1" -UpdateProjects -DryRun
    
    Simulates updating project files to version 1.0.1-preview.1 without making changes.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Version,
    
    [switch]$UpdateProjects,
    
    [switch]$DryRun,
    
    [switch]$Force
)

# Validate version format
if ($Version -notmatch '^(\d+)\.(\d+)\.(\d+)(?:-([a-zA-Z0-9\-\.]+))?$') {
    Write-Error "Invalid version format. Use semantic versioning (e.g., 1.0.0, 1.0.1-preview.1)"
    exit 1
}

# Check if we're in the correct directory
$slnFile = "vscode-restclient-dotnet.slnx"
if (-not (Test-Path $slnFile)) {
    Write-Error "This script must be run from the repository root directory (where $slnFile is located)"
    exit 1
}

# Check for uncommitted changes
$gitStatus = git status --porcelain
if ($gitStatus -and -not $Force) {
    Write-Error "There are uncommitted changes. Commit them first or use -Force to continue."
    Write-Host "Uncommitted changes:"
    $gitStatus | ForEach-Object { Write-Host "  $_" }
    exit 1
}

# Project files to update
$projectFiles = @(
    "src\RESTClient.NET.Core\RESTClient.NET.Core.csproj",
    "src\RESTClient.NET.Testing\RESTClient.NET.Testing.csproj"
)

function Update-ProjectVersion {
    param(
        [string]$ProjectFile,
        [string]$NewVersion
    )
    
    if (-not (Test-Path $ProjectFile)) {
        Write-Error "Project file not found: $ProjectFile"
        return $false
    }
    
    $content = Get-Content $ProjectFile -Raw
    $newContent = $content -replace '<PackageVersion>.*</PackageVersion>', "<PackageVersion>$NewVersion</PackageVersion>"
    
    if ($content -eq $newContent) {
        Write-Warning "No version found to update in $ProjectFile"
        return $false
    }
    
    if (-not $DryRun) {
        Set-Content $ProjectFile -Value $newContent -Encoding UTF8
    }
    
    Write-Host "✅ Updated $ProjectFile to version $NewVersion" -ForegroundColor Green
    return $true
}

function Test-Build {
    Write-Host "🔨 Testing build..." -ForegroundColor Yellow
    
    if ($DryRun) {
        Write-Host "  (Dry run - skipping actual build)" -ForegroundColor Gray
        return $true
    }
    
    $buildResult = dotnet build $slnFile --configuration Release --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed. Fix build errors before releasing."
        return $false
    }
    
    Write-Host "✅ Build successful" -ForegroundColor Green
    return $true
}

function Test-Suite {
    Write-Host "🧪 Running tests..." -ForegroundColor Yellow
    
    if ($DryRun) {
        Write-Host "  (Dry run - skipping actual tests)" -ForegroundColor Gray
        return $true
    }
    
    $testResult = dotnet test $slnFile --configuration Release --verbosity minimal --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Tests failed. Fix test failures before releasing."
        return $false
    }
    
    Write-Host "✅ All tests passed" -ForegroundColor Green
    return $true
}

function Create-GitTag {
    param([string]$TagVersion)
    
    $tagName = "v$TagVersion"
    
    # Check if tag already exists
    $existingTag = git tag -l $tagName
    if ($existingTag) {
        Write-Error "Tag $tagName already exists. Use a different version or delete the existing tag."
        return $false
    }
    
    if ($DryRun) {
        Write-Host "  (Dry run - would create tag: $tagName)" -ForegroundColor Gray
        return $true
    }
    
    # Commit version changes if any
    if ($UpdateProjects -and (git status --porcelain)) {
        git add .
        git commit -m "chore: bump version to $TagVersion"
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to commit version changes"
            return $false
        }
        Write-Host "✅ Committed version changes" -ForegroundColor Green
    }
    
    # Create and push tag
    git tag $tagName
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to create tag $tagName"
        return $false
    }
    
    git push origin $tagName
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to push tag $tagName"
        return $false
    }
    
    Write-Host "✅ Created and pushed tag: $tagName" -ForegroundColor Green
    return $true
}

# Main execution
Write-Host "🚀 Starting release process for version $Version" -ForegroundColor Cyan

if ($DryRun) {
    Write-Host "🧪 DRY RUN MODE - No changes will be made" -ForegroundColor Yellow
}

# Update project versions if requested
if ($UpdateProjects) {
    Write-Host "📝 Updating project versions..." -ForegroundColor Yellow
    
    $success = $true
    foreach ($projectFile in $projectFiles) {
        if (-not (Update-ProjectVersion -ProjectFile $projectFile -NewVersion $Version)) {
            $success = $false
        }
    }
    
    if (-not $success) {
        Write-Error "Failed to update all project versions"
        exit 1
    }
}

# Test build
if (-not (Test-Build)) {
    exit 1
}

# Run tests
if (-not (Test-Suite)) {
    exit 1
}

# Create git tag and trigger release
if (-not (Create-GitTag -TagVersion $Version)) {
    exit 1
}

if (-not $DryRun) {
    Write-Host ""
    Write-Host "🎉 Release process completed successfully!" -ForegroundColor Green
    Write-Host "   📦 Packages will be published automatically via GitHub Actions" -ForegroundColor Cyan
    Write-Host "   🔗 Monitor progress at: https://github.com/Meir017/vscode-restclient-dotnet/actions" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "✅ Dry run completed successfully!" -ForegroundColor Green
    Write-Host "   Run without -DryRun to execute the actual release" -ForegroundColor Cyan
    Write-Host ""
}
