# NuGet Publishing Setup Script
# Run this script to verify your setup before publishing

Write-Host "?? Email.Attachments - NuGet Publishing Setup" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Check .NET SDK
Write-Host "?? Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? .NET SDK: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "   ? .NET SDK not found. Please install .NET 10.0 SDK" -ForegroundColor Red
    exit 1
}

# Check project file
Write-Host ""
Write-Host "?? Checking project file..." -ForegroundColor Yellow
$projectPath = "EmailFilesDownloader/Email.Attachments/Email.Attachments.csproj"
if (Test-Path $projectPath) {
    Write-Host "   ? Project file found: $projectPath" -ForegroundColor Green
} else {
    Write-Host "   ? Project file not found: $projectPath" -ForegroundColor Red
    exit 1
}

# Build project
Write-Host ""
Write-Host "?? Building project..." -ForegroundColor Yellow
dotnet build $projectPath --configuration Release
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? Build successful" -ForegroundColor Green
} else {
    Write-Host "   ? Build failed" -ForegroundColor Red
    exit 1
}

# Pack project
Write-Host ""
Write-Host "?? Creating NuGet package..." -ForegroundColor Yellow
dotnet pack $projectPath --configuration Release --output ./test-packages
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? Package created successfully" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Package details:" -ForegroundColor Cyan
    Get-ChildItem ./test-packages/*.nupkg | ForEach-Object {
        Write-Host "   ?? $($_.Name)" -ForegroundColor White
        Write-Host "      Size: $([math]::Round($_.Length / 1KB, 2)) KB" -ForegroundColor Gray
    }
} else {
    Write-Host "   ? Pack failed" -ForegroundColor Red
    exit 1
}

# Check GitHub secrets
Write-Host ""
Write-Host "?? GitHub Secrets Checklist:" -ForegroundColor Yellow
Write-Host "   ?? NUGET_API_KEY - You need to add this manually to GitHub" -ForegroundColor White
Write-Host ""
Write-Host "   Steps to add GitHub secret:" -ForegroundColor Gray
Write-Host "   1. Go to: https://github.com/vpaulino/InvoicesManagement/settings/secrets/actions" -ForegroundColor Gray
Write-Host "   2. Click 'New repository secret'" -ForegroundColor Gray
Write-Host "   3. Name: NUGET_API_KEY" -ForegroundColor Gray
Write-Host "   4. Value: Your NuGet API key from https://www.nuget.org/account/apikeys" -ForegroundColor Gray

# Check workflow files
Write-Host ""
Write-Host "?? Checking GitHub Actions workflows..." -ForegroundColor Yellow
$workflows = @(
    ".github/workflows/publish-nuget.yml",
    ".github/workflows/ci-build.yml"
)

foreach ($workflow in $workflows) {
    if (Test-Path $workflow) {
        Write-Host "   ? $workflow" -ForegroundColor Green
    } else {
        Write-Host "   ? $workflow - Missing!" -ForegroundColor Red
    }
}

# Summary
Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "? Setup Verification Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "?? Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Get your NuGet API key from: https://www.nuget.org/account/apikeys" -ForegroundColor White
Write-Host "   2. Add NUGET_API_KEY secret to GitHub repository" -ForegroundColor White
Write-Host "   3. Create and push a version tag: git tag v1.0.0 && git push origin v1.0.0" -ForegroundColor White
Write-Host "   4. Monitor GitHub Actions: https://github.com/vpaulino/InvoicesManagement/actions" -ForegroundColor White
Write-Host "   5. Check package on NuGet.org after publication" -ForegroundColor White
Write-Host ""
Write-Host "?? Documentation:" -ForegroundColor Yellow
Write-Host "   - NUGET_PUBLISHING_GUIDE.md - Complete publishing guide" -ForegroundColor White
Write-Host "   - Email.Attachments/CHANGELOG.md - Version history" -ForegroundColor White
Write-Host ""
Write-Host "?? Happy Publishing!" -ForegroundColor Cyan

# Cleanup
Write-Host ""
Write-Host "?? Cleaning up test packages..." -ForegroundColor Gray
Remove-Item -Path ./test-packages -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "   ? Cleanup complete" -ForegroundColor Green
