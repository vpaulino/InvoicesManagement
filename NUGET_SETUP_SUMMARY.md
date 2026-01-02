# ?? NuGet Publishing Setup - Complete!

## ? What Was Configured

Your repository is now fully configured to publish `Email.Attachments` to NuGet.org via GitHub Actions!

---

## ?? Files Created/Updated

### **Project Configuration**
1. ? `Email.Attachments/Email.Attachments.csproj` - Updated with NuGet metadata
   - Package ID, version, authors
   - Description, tags, license
   - SourceLink integration for debugging
   - Symbol package (.snupkg) generation

### **GitHub Actions Workflows**
2. ? `.github/workflows/publish-nuget.yml` - Automated publishing
   - Triggered by git tags (`v*.*.*`)
   - Manual trigger option
   - Publishes to NuGet.org
   - Creates GitHub releases

3. ? `.github/workflows/ci-build.yml` - Continuous integration
   - Builds on push to main branches
   - Validates package creation
   - Runs on pull requests

### **Documentation**
4. ? `NUGET_PUBLISHING_GUIDE.md` - Complete setup and publishing guide
5. ? `Email.Attachments/PACKAGE_README.md` - Package documentation for NuGet.org
6. ? `Email.Attachments/CHANGELOG.md` - Version history
7. ? `setup-nuget-publishing.ps1` - Verification script
8. ? `NUGET_SETUP_SUMMARY.md` - This file

---

## ?? How to Publish

### **Initial Setup (One-Time)**

#### **1. Get NuGet API Key**
1. Go to https://www.nuget.org/
2. Sign in with your account
3. Navigate to: **Account** ? **API Keys**
4. Click **Create**
5. Configure:
   - **Key Name:** `GitHub Actions - InvoicesManagement`
   - **Glob Pattern:** `Email.Attachments*`
   - **Select Scopes:** `Push new packages and package versions`
6. Click **Create** and **copy the key** (you won't see it again!)

#### **2. Add to GitHub Secrets**
1. Go to: https://github.com/vpaulino/InvoicesManagement/settings/secrets/actions
2. Click **New repository secret**
3. Add secret:
   - **Name:** `NUGET_API_KEY`
   - **Value:** (paste your NuGet API key)
4. Click **Add secret**

#### **3. Verify Setup**
Run the verification script:
```powershell
.\setup-nuget-publishing.ps1
```

---

### **Publishing a New Version**

#### **Method 1: Git Tag (Recommended)**

```bash
# Update version in Email.Attachments.csproj if needed
# Update CHANGELOG.md with release notes

# Commit changes
git add .
git commit -m "Release v1.0.0"

# Create and push tag
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin main
git push origin v1.0.0
```

**What happens automatically:**
1. ? GitHub Actions detects the tag
2. ? Builds the project
3. ? Creates NuGet package
4. ? Publishes to NuGet.org
5. ? Creates GitHub Release
6. ? Attaches package to release

#### **Method 2: Manual Trigger**

1. Go to: https://github.com/vpaulino/InvoicesManagement/actions
2. Select **Publish NuGet Package** workflow
3. Click **Run workflow**
4. Enter version (e.g., `1.0.1`)
5. Click **Run workflow**

---

## ?? Workflow Overview

### **Publish NuGet Workflow** (`.github/workflows/publish-nuget.yml`)

**Triggers:**
- Git tags: `v1.0.0`, `v1.2.3`, etc.
- Manual trigger via GitHub UI

**Steps:**
1. ? Checkout code
2. ? Setup .NET 10.0
3. ? Extract version from tag
4. ? Restore dependencies
5. ? Build (Release configuration)
6. ? Run tests (if any)
7. ? Pack NuGet package
8. ? Push to NuGet.org
9. ? Create GitHub Release
10. ? Upload artifacts

### **CI Build Workflow** (`.github/workflows/ci-build.yml`)

**Triggers:**
- Push to `main`, `master`, `develop`, `features/**`
- Pull requests to `main`, `master`, `develop`

**Steps:**
1. ? Checkout code
2. ? Setup .NET 10.0
3. ? Restore dependencies
4. ? Build
5. ? Run tests
6. ? Pack (validation only)
7. ? Upload artifacts

---

## ?? Package Metadata

```xml
<PackageId>Email.Attachments</PackageId>
<Version>1.0.0</Version>
<Authors>vpaulino</Authors>
<Description>
  Comprehensive email attachment extraction and management library for .NET.
  Extract, filter, and persist email attachments from Gmail with pluggable
  storage backends, rich date filtering, and use-case oriented API.
</Description>
<PackageTags>
  email;gmail;attachments;extraction;automation;invoice;pdf;storage;oauth;api
</PackageTags>
<PackageLicenseExpression>MIT</PackageLicenseExpression>
```

---

## ?? Pre-Release Checklist

Before publishing a new version:

- [ ] Update version in `Email.Attachments.csproj`
- [ ] Update `CHANGELOG.md` with changes
- [ ] Update `PackageReleaseNotes` in `.csproj`
- [ ] Test locally: `dotnet build` and `dotnet pack`
- [ ] Commit all changes
- [ ] Create git tag with version
- [ ] Push changes and tag
- [ ] Monitor GitHub Actions workflow
- [ ] Verify package on NuGet.org
- [ ] Test installation: `dotnet add package Email.Attachments`

---

## ?? Version Numbering

Follow [Semantic Versioning](https://semver.org/):

```
MAJOR.MINOR.PATCH[-PRERELEASE]

Examples:
- 1.0.0       ? First stable release
- 1.1.0       ? New features (backward compatible)
- 1.1.1       ? Bug fixes
- 1.0.0-beta  ? Pre-release
- 2.0.0       ? Breaking changes
```

**To update version:**
Edit `Email.Attachments/Email.Attachments.csproj`:
```xml
<Version>1.0.0</Version>
```

---

## ?? Monitoring & Verification

### **GitHub Actions**
- View runs: https://github.com/vpaulino/InvoicesManagement/actions
- Check logs for errors
- Download artifacts

### **NuGet.org**
- Your packages: https://www.nuget.org/profiles/vpaulino
- Package page: https://www.nuget.org/packages/Email.Attachments
- Statistics and downloads

### **Package Installation**
```bash
# Test installation
dotnet new console -n TestApp
cd TestApp
dotnet add package Email.Attachments
dotnet build
```

---

## ?? Troubleshooting

### **Build Fails**
- Check GitHub Actions logs
- Verify .NET 10.0 SDK
- Test locally: `dotnet build`

### **Package Already Exists**
- Increment version number
- NuGet doesn't allow overwriting versions

### **Invalid API Key**
- Regenerate on NuGet.org
- Update GitHub secret

### **Package Not Appearing**
- Wait 15-30 minutes for NuGet indexing
- Check for validation errors on NuGet.org

---

## ?? Documentation Files

| File | Purpose |
|------|---------|
| `NUGET_PUBLISHING_GUIDE.md` | Detailed publishing instructions |
| `Email.Attachments/PACKAGE_README.md` | Shown on NuGet.org package page |
| `Email.Attachments/CHANGELOG.md` | Version history |
| `setup-nuget-publishing.ps1` | Setup verification script |
| `.github/workflows/publish-nuget.yml` | Publishing automation |
| `.github/workflows/ci-build.yml` | CI automation |

---

## ?? Quick Commands

```bash
# Verify setup
.\setup-nuget-publishing.ps1

# Test build locally
dotnet build EmailFilesDownloader/Email.Attachments/Email.Attachments.csproj -c Release

# Test pack locally
dotnet pack EmailFilesDownloader/Email.Attachments/Email.Attachments.csproj -c Release

# Create release tag
git tag -a v1.0.0 -m "Release 1.0.0"
git push origin v1.0.0

# View GitHub Actions
start https://github.com/vpaulino/InvoicesManagement/actions

# View NuGet package (after publish)
start https://www.nuget.org/packages/Email.Attachments
```

---

## ? Success Criteria

After successful publish:

1. ? GitHub Actions workflow shows green checkmark
2. ? Package visible on https://www.nuget.org/packages/Email.Attachments
3. ? GitHub Release created with package attached
4. ? Package installable via `dotnet add package Email.Attachments`
5. ? README visible on NuGet package page
6. ? Download count incrementing

---

## ?? Security Notes

- ? Never commit `NUGET_API_KEY` to repository
- ? Use GitHub Secrets for sensitive data
- ? Limit API key scope to Email.Attachments package
- ? Rotate API keys periodically
- ? Review GitHub Actions permissions

---

## ?? Support

- **Documentation:** See `NUGET_PUBLISHING_GUIDE.md`
- **Issues:** https://github.com/vpaulino/InvoicesManagement/issues
- **GitHub Actions:** https://github.com/vpaulino/InvoicesManagement/actions

---

## ?? Next Steps

1. **Run setup script** to verify everything is configured:
   ```powershell
   .\setup-nuget-publishing.ps1
   ```

2. **Get your NuGet API key** from NuGet.org

3. **Add GitHub secret** with your API key

4. **Create and push a tag** to publish:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

5. **Monitor the workflow** in GitHub Actions

6. **Celebrate** when your package is live on NuGet.org! ??

---

**Your Email.Attachments package is ready for the world! ????**
