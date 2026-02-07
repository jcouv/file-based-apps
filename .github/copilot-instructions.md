# File-Based .NET apps

This repo contains file-based .NET apps. These are standalone `.cs` files that can be run directly without creating a project file or traditional project structure.

## Key Features

- **No project file required** - Run C# code directly from a `.cs` file
- **Built-in CLI integration** - Uses the standard `dotnet` CLI, no additional tools needed
- **Scales to full projects** - Can be converted to traditional project-based apps when needed
- **Same C# language** - Full C# language support, same compiler and runtime
- **Cross-platform scripting** - Supports shebang lines for Unix-like systems

## Running File-Based Apps

### Basic Execution

```bash
dotnet run app.cs
# Or explicitly use --file option to avoid fallback behavior
dotnet run --file app.cs
# Or use the shortcut (if app.cs exists)
dotnet app.cs
```

### With Shebang (Unix-like systems)

Add this line at the top of your `.cs` file:

```csharp
#!/usr/bin/env dotnet
```

Then make it executable and run directly:

```bash
chmod +x app.cs
./app.cs
```

## File-Level Directives

File-based apps support several directives that configure the build without needing a project file:

### Adding NuGet Packages

Use the `#:package` directive at the top of your file:

```csharp
#:package Humanizer@2.14.1
#:package Microsoft.AspNetCore.OpenApi@10.*-*

using Humanizer;
// Your code here...
```

**Alternative CLI method:**

```bash
dotnet package add <PackageId> --file yourfile.cs
dotnet package remove <PackageId> --file yourfile.cs
```

### Specifying SDK

By default, file-based apps use `Microsoft.NET.Sdk`. For web applications, use:

```csharp
#:sdk Microsoft.NET.Sdk.Web
```

### Setting MSBuild Properties

Configure build properties using `#:property`:

```csharp
#:property LangVersion=preview
#:property TargetFramework=net10.0
```

Start with these build properties unless otherwise specified:
```csharp
#:property LangVersion=preview
#:property PublishAot=false
```

### Project References (.NET 10 Preview 6+)

Reference other projects using `#:project`:

```csharp
#:project ../MyLibrary/MyLibrary.csproj
```

## Building File-Based apps

File-based apps can be built like regular projects:

```bash
dotnet build app.cs
```

**Note:** Multi-file support is postponed for .NET 11. In .NET 10, only the single file passed as the command-line argument is part of the compilation.

## Additional Commands

File-based apps support several `dotnet` commands:

```bash
dotnet build app.cs     # Build the program
dotnet publish app.cs   # Publish (Native AOT by default)
dotnet pack app.cs      # Create NuGet package
dotnet clean app.cs     # Clean build artifacts
dotnet restore app.cs   # Restore dependencies
```

**Note:** File-based apps have `PublishAot=true` set by default. To opt out, use `#:property PublishAot=false` directive.

## Converting to Project-Based Program

DO NOT perform this action without explicit consent to convert to a project-based program.

When your file-based program grows in complexity, convert it to a traditional project:

```bash
dotnet project convert app.cs
```

This command:

- Creates a new directory named after your file
- Generates a `.csproj` file with all directives translated to MSBuild properties
- Moves your code to `Program.cs`
- Preserves all package references and settings

## Example: Web API

```csharp
#!/usr/bin/dotnet run
#:sdk Microsoft.NET.Sdk.Web
#:package Microsoft.AspNetCore.OpenApi@10.*-*

var builder = WebApplication.CreateBuilder();
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapGet("/", () => "Hello, world!");
app.Run();
```

## Build Artifacts

Build outputs are placed in a temporary directory unique to each user and hashed by file path. This:

- Keeps source directories clean
- Avoids conflicts between multiple users
- Enables artifact reuse for better performance

Artifacts are automatically cleaned every 2 days (removes artifacts unused for 30+ days). Manual cleanup:

```bash
dotnet clean-file-based-app-artifacts
```

## Best Practices

1. **Start simple** - Use file-based apps for prototyping, learning, and small scripts
2. **Use directives** - Leverage `#:package`, `#:sdk`, and `#:property` directives for configuration
3. **Convert when needed** - Use `dotnet project convert` when your program outgrows a single file
4. **Cross-platform scripts** - Use shebang lines for executable C# scripts on Unix-like systems
5. **Version pinning** - Specify exact package versions for reproducible builds
6. **Performance** - Use `--no-cache` to force full rebuild if needed

## Important Guidelines

- **Requires .NET 10 Preview 4+** - This feature is not available in earlier versions
- **File extension required** - The file must have a `.cs` extension or start with `#!`
- **VS Code support** - Install C# Dev Kit and switch to pre-release version (2.79.8+) for full support
- **Directive order** - Place all `#:` directives at the top of the file before any code
- **Fallback behavior** - If a project file exists in the current directory, `dotnet run file.cs` will pass `file.cs` as an argument to the project instead. Use `--file` option to avoid this
- **Entry point required** - The file must contain top-level statements to be executable
