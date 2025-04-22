# Set your base directory
$baseDir = "CachingLibrary"
mkdir $baseDir
cd $baseDir

# Create the solution
dotnet new sln -n CachingLibrary

# Create projects
dotnet new classlib -n CachingLibrary.Domain
dotnet new classlib -n CachingLibrary.Application
dotnet new classlib -n CachingLibrary.Infrastructure.Sql
dotnet new classlib -n CachingLibrary.Infrastructure.Redis
dotnet new classlib -n CachingLibrary.Infrastructure.Memory
dotnet new classlib -n CachingLibrary.Abstractions
dotnet new xunit    -n CachingLibrary.Tests

# Add projects to the solution
dotnet sln add .\CachingLibrary.Domain\CachingLibrary.Domain.csproj
dotnet sln add .\CachingLibrary.Application\CachingLibrary.Application.csproj
dotnet sln add .\CachingLibrary.Infrastructure.Sql\CachingLibrary.Infrastructure.Sql.csproj
dotnet sln add .\CachingLibrary.Infrastructure.Redis\CachingLibrary.Infrastructure.Redis.csproj
dotnet sln add .\CachingLibrary.Infrastructure.Memory\CachingLibrary.Infrastructure.Memory.csproj
dotnet sln add .\CachingLibrary.Abstractions\CachingLibrary.Abstractions.csproj
dotnet sln add .\CachingLibrary.Tests\CachingLibrary.Tests.csproj

# Set project references (fixed paths)
dotnet add .\CachingLibrary.Domain\CachingLibrary.Domain.csproj reference .\CachingLibrary.Abstractions\CachingLibrary.Abstractions.csproj
dotnet add .\CachingLibrary.Application\CachingLibrary.Application.csproj reference .\CachingLibrary.Domain\CachingLibrary.Domain.csproj
dotnet add .\CachingLibrary.Infrastructure.Sql\CachingLibrary.Infrastructure.Sql.csproj reference .\CachingLibrary.Abstractions\CachingLibrary.Abstractions.csproj
dotnet add .\CachingLibrary.Infrastructure.Redis\CachingLibrary.Infrastructure.Redis.csproj reference .\CachingLibrary.Abstractions\CachingLibrary.Abstractions.csproj
dotnet add .\CachingLibrary.Infrastructure.Memory\CachingLibrary.Infrastructure.Memory.csproj reference .\CachingLibrary.Abstractions\CachingLibrary.Abstractions.csproj
dotnet add .\CachingLibrary.Tests\CachingLibrary.Tests.csproj reference .\CachingLibrary.Abstractions\CachingLibrary.Abstractions.csproj
dotnet add .\CachingLibrary.Tests\CachingLibrary.Tests.csproj reference .\CachingLibrary.Infrastructure.Memory\CachingLibrary.Infrastructure.Memory.csproj
