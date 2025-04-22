# CachingLibrary

A modular, lightweight caching library built using clean architecture principles and designed to support multiple caching strategies in .NET 8 applications. This solution is NuGet-pack-ready and tested end-to-end for Redis, SQL Server, and in-memory backends.

## ✅ What This Is
This is a multi-project solution structured to build and publish pluggable caching implementations, each packaged as a NuGet package:

- **CachingLibrary.Abstractions**: Core interfaces and base contracts
- **CachingLibrary.Infrastructure.Memory**: In-memory cache using `Microsoft.Extensions.Caching.Memory`
- **CachingLibrary.Infrastructure.Redis**: Distributed cache using `StackExchange.Redis`
- **CachingLibrary.Infrastructure.Sql**: SQL Server cache using `Dapper` and memory-optimized tables
- **CachingLibrary.Tests**: Unit tests for all providers

---

## 🧱 What You Did

1. **Implemented Abstractions**
   - `ICacheStore<TKey, TValue>` for uniform access to all providers

2. **Created Pluggable Providers**
   - Memory: `MemoryCacheStore`
   - Redis: `RedisCacheStore`
   - SQL: `SqlCacheStore`

3. **Structured the Solution**
   - Modularized code into separate projects for packaging
   - Wrote DI extension methods for each provider

4. **Documented Everything**
   - XML comments on all public types
   - PhD-level inline documentation

5. **Set Up Projects for NuGet Packaging**
   - Added NuGet metadata to each `.csproj`
   - Set `<GeneratePackageOnBuild>true>`

6. **Built and Packed Locally**
   Open your terminal and run:
   ```bash
   cd C:\Projects\RCS\RCS.CachingLibrary

   dotnet build -c Release
   dotnet pack -c Release

   dotnet build -c Release
   dotnet pack -c Release -o ./nupkgs
   ```

7. **Verified Output**
   - Ensured `.nupkg` files were generated:
     ```
     /nupkgs
     ├── CachingLibrary.Abstractions.1.0.0.nupkg
     ├── CachingLibrary.Memory.1.0.0.nupkg
     ├── CachingLibrary.Redis.1.0.0.nupkg
     └── CachingLibrary.Sql.1.0.0.nupkg
     ```

---

## 🚀 How to Use This

### 1. Reference From Another Project
```bash
dotnet add package CachingLibrary.Memory --source ./nupkgs
```

### 2. Register in .NET 8 Minimal API
```csharp
builder.Services.AddInMemoryCache<string, MyDto>();
```

### 3. Use Anywhere via DI
```csharp
public class MyService(ICacheStore<string, MyDto> cache)
{
    public async Task DoStuffAsync()
    {
        await cache.SetAsync("key", new MyDto(), TimeSpan.FromMinutes(5));
        var value = await cache.GetAsync("key");
    }
}
```

---

## 🧪 Run Unit Tests
```bash
dotnet test
```

---

## 📤 Publish to NuGet.org (Optional)
```bash
dotnet nuget push ./nupkgs/CachingLibrary.*.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

---

## 📎 License
MIT — do what you want, just don’t remove credit.

---

## ✅ You're Ready
You now have:
- Modular, versioned, documented NuGet packages
- Pluggable caching with DI
- Working tests and easy adoption in any .NET 8 API

Need CI/CD or NuGet publish automation? Just ask.

---

## 📤 Publish to GitHub Packages (Optional)

### 1. Create a GitHub Personal Access Token (PAT)
- Go to https://github.com/settings/tokens
- Generate a token with:
  - `write:packages`
  - `read:packages`
  - `repo` (if private)

### 2. Add a `nuget.config` to the root of your solution
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/YOUR_USERNAME/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_PAT" />
    </github>
  </packageSourceCredentials>
</configuration>
```

### 3. Push a package
```bash
dotnet nuget push ./nupkgs/CachingLibrary.Memory.1.0.0.nupkg \
  --source github \
  --api-key YOUR_GITHUB_PAT
```

---

## 📤 Publish to Azure DevOps Artifacts (Optional)

### 1. Create a Feed
- Go to Azure DevOps → Artifacts → Create Feed
- Note your feed URL:
  ```
  https://pkgs.dev.azure.com/YOUR_ORG/_packaging/YOUR_FEED_NAME/nuget/v3/index.json
  ```

### 2. Add a `nuget.config` to your solution
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="AzureDevOps" value="https://pkgs.dev.azure.com/YOUR_ORG/_packaging/YOUR_FEED_NAME/nuget/v3/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <AzureDevOps>
      <add key="Username" value="AzureDevOps" />
      <add key="ClearTextPassword" value="YOUR_AZURE_DEVOPS_PAT" />
    </AzureDevOps>
  </packageSourceCredentials>
</configuration>
```

### 3. Push a package
```bash
dotnet nuget push ./nupkgs/CachingLibrary.Memory.1.0.0.nupkg \
  --source AzureDevOps
```

---

## 📥 Install Locally in Another Project

If you want to use your built packages locally in another .NET project:

### 1. Ensure the `.nupkg` exists
From your library solution, run:
```bash
dotnet pack -c Release -o ./nupkgs
```

This will create files like:
```
./nupkgs/CachingLibrary.Memory.1.0.0.nupkg
```

### 2. Install it in the consumer project
In your other project's directory:
```bash
dotnet add package CachingLibrary.Memory --source ../RCS.CachingLibrary/nupkgs
```

Make sure to adjust the relative path to the `nupkgs` folder if needed.

### 3. Restore and confirm
```bash
dotnet restore
```

The package will be installed directly from the local path.