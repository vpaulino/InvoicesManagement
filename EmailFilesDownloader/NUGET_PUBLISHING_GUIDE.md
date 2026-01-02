# ?? NuGet Package Publishing Guide

## ?? Overview

This repository is configured to automatically publish the `Email.Attachments` NuGet package to NuGet.org using GitHub Actions.

---

## ?? Initial Setup

### **1. Get Your NuGet API Key**

1. Go to [NuGet.org](https://www.nuget.org/)
2. Sign in with your account
3. Click your username ? **API Keys**
4. Click **Create** to generate a new API key
   - **Key Name:** `GitHub Actions - InvoicesManagement`
   - **Select Scopes:** `Push new packages and package versions`
   - **Select Packages:** Choose `Email.Attachments` (or leave as "All")
   - **Glob Pattern:** `Email.Attachments*`
5. Click **Create**
6. **Copy the API key** (you won't see it again!)

---

### **2. Add API Key to GitHub Secrets**

1. Go to your GitHub repository: `https://github.com/vpaulino/InvoicesManagement`
2. Navigate to **Settings** ? **Secrets and variables** ? **Actions**
3. Click **New repository secret**
4. Add the secret:
   - **Name:** `NUGET_API_KEY`
   - **Value:** Paste your NuGet API key
5. Click **Add secret**

---

## ?? Publishing a New Version

### **Method 1: Git Tag (Recommended)**

This is the recommended approach for versioned releases.

```bash
# Create and push a version tag
git tag v1.0.0
git push origin v1.0.0

# Or create an annotated tag with message
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

**What happens:**
1. GitHub Actions detects the tag
2. Extracts version from tag (e.g., `v1.0.0` ? `1.0.0`)
3. Builds the package with that version
4. Publishes to NuGet.org
5. Creates a GitHub Release with the package attached

---

### **Method 2: Manual Trigger**

For testing or non-standard versions:

1. Go to **Actions** tab in GitHub
2. Select **Publish NuGet Package** workflow
3. Click **Run workflow**
4. Enter the version number (e.g., `1.0.1-beta`)
5. Click **Run workflow**

---

## ?? Version Numbering

Follow [Semantic Versioning](https://semver.org/):

```
MAJOR.MINOR.PATCH[-PRERELEASE]

Examples:
- 1.0.0       ? First stable release
- 1.1.0       ? New features, backward compatible
- 1.1.1       ? Bug fixes
- 1.0.0-beta  ? Pre-release version
- 2.0.0       ? Breaking changes
```

### **Updating Version:**

Edit `Email.Attachments/Email.Attachments.csproj`:

```xml
<Version>1.0.0</Version>  <!-- Update this -->
```

**Note:** When using git tags, the version in the tag takes precedence.

---

## ?? Workflow Overview

### **CI Build Workflow** (`ci-build.yml`)

**Triggers:**
- Push to `main`, `master`, `develop`, or `features/**` branches
- Pull requests to `main`, `master`, `develop`

**What it does:**
- ? Builds the project
- ? Runs tests (if any)
- ? Validates package creation
- ? Uploads artifacts for inspection

### **Publish NuGet Workflow** (`publish-nuget.yml`)

**Triggers:**
- Push tags matching `v*.*.*` (e.g., `v1.0.0`)
- Manual trigger via GitHub Actions UI

**What it does:**
- ? Builds the project
- ? Creates NuGet package (.nupkg)
- ? Publishes to NuGet.org
- ? Creates GitHub Release
- ? Attaches package to release

---

## ?? Release Checklist

Before publishing a new version:

- [ ] Update `CHANGELOG.md` with changes
- [ ] Update version in `Email.Attachments.csproj` (if not using git tags)
- [ ] Update `PackageReleaseNotes` in `.csproj`
- [ ] Ensure all tests pass locally
- [ ] Commit and push changes
- [ ] Create and push version tag
- [ ] Monitor GitHub Actions workflow
- [ ] Verify package on NuGet.org
- [ ] Test installation: `dotnet add package Email.Attachments`

---

## ?? Package Metadata

Current configuration in `Email.Attachments.csproj`:

```xml
<PackageId>Email.Attachments</PackageId>
<Version>1.0.0</Version>
<Authors>vpaulino</Authors>
<Description>Comprehensive email attachment extraction and management library...</Description>
<PackageTags>email;gmail;attachments;extraction;automation;invoice;pdf;storage;oauth;api</PackageTags>
<PackageProjectUrl>https://github.com/vpaulino/InvoicesManagement</PackageProjectUrl>
<PackageLicenseExpression>MIT</PackageLicenseExpression>
```

To update metadata, edit these properties in the `.csproj` file.

---

## ?? Troubleshooting

### **Issue: "Package already exists"**

**Solution:** Increment the version number. NuGet doesn't allow overwriting existing versions.

### **Issue: "Invalid API Key"**

**Solution:** 
1. Regenerate API key on NuGet.org
2. Update `NUGET_API_KEY` secret in GitHub

### **Issue: "Build failed"**

**Solution:**
1. Check the Actions tab for detailed error logs
2. Ensure the project builds locally: `dotnet build`
3. Verify .NET 10.0 SDK is used in workflow

### **Issue: "Package not showing on NuGet.org"**

**Solution:** 
- NuGet indexing can take 15-30 minutes
- Check your NuGet.org account for validation errors
- Verify the package isn't unlisted

---

## ?? Monitoring

### **GitHub Actions**
- View workflow runs: `https://github.com/vpaulino/InvoicesManagement/actions`
- Check build logs for errors
- Download artifacts for inspection

### **NuGet.org**
- View your packages: `https://www.nuget.org/profiles/vpaulino`
- Check package statistics and downloads
- Manage package versions

---

## ?? Security Best Practices

1. ? **Never commit API keys** to the repository
2. ? Use GitHub Secrets for sensitive data
3. ? Limit API key scope to specific packages
4. ? Rotate API keys periodically
5. ? Review workflow permissions regularly

---

## ?? Additional Resources

- [NuGet Documentation](https://docs.microsoft.com/en-us/nuget/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Semantic Versioning](https://semver.org/)
- [Package Icon Guidelines](https://docs.microsoft.com/en-us/nuget/reference/nuspec#icon)

---

## ?? Quick Commands

```bash
# Local pack (testing)
dotnet pack EmailFilesDownloader/Email.Attachments/Email.Attachments.csproj -c Release

# Local publish (testing)
dotnet nuget push ./bin/Release/Email.Attachments.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# Create release tag
git tag -a v1.0.0 -m "Release 1.0.0"
git push origin v1.0.0

# Delete tag (if needed)
git tag -d v1.0.0
git push origin :refs/tags/v1.0.0
```

---

## ? Success Criteria

After successful publish, you should see:

1. ? Green checkmark on GitHub Actions workflow
2. ? Package listed on https://www.nuget.org/packages/Email.Attachments
3. ? GitHub Release created with package attached
4. ? Package installable via: `dotnet add package Email.Attachments`

---

**Happy Publishing! ????**
