#:package Microsoft.NETCore.ILDAsm@10.0.0-rc.2.25502.107

using System.Diagnostics;
using System.Runtime.InteropServices;

const string PackageVersion = "10.0.0-rc.2.25502.107";

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: dotnet run run-ildasm.cs <assembly> [ildasm switches]");
    return 1;
}

var rid = RuntimeInformation.OSArchitecture switch
{
    Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win-x64",
    Architecture.X86 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win-x86",
    Architecture.Arm when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win-arm",
    Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win-arm64",
    Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "linux-x64",
    Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "linux-arm64",
    Architecture.X64 when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "osx-x64",
    Architecture.Arm64 when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "osx-arm64",
    _ => throw new PlatformNotSupportedException($"Unsupported platform/architecture for ILDASM.")
};

var packageId = $"runtime.{rid}.microsoft.netcore.ildasm";
var nugetRoot = Environment.GetEnvironmentVariable("NUGET_PACKAGES")
    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
var packageRoot = Path.Combine(nugetRoot, packageId, PackageVersion);

if (!Directory.Exists(packageRoot))
{
    // Try to find any version of the package
    var packageDir = Path.Combine(nugetRoot, packageId);
    if (Directory.Exists(packageDir))
    {
        var versions = Directory.GetDirectories(packageDir).Select(Path.GetFileName).OrderDescending().ToArray();
        if (versions.Length > 0)
        {
            packageRoot = Path.Combine(packageDir, versions[0]!);
            Console.Error.WriteLine($"Warning: Using version {versions[0]} instead of {PackageVersion}");
        }
    }
    
    if (!Directory.Exists(packageRoot))
    {
        Console.Error.WriteLine($"ILDASM package not found at {packageRoot}.");
        Console.Error.WriteLine($"The package should be automatically restored when running this script.");
        Console.Error.WriteLine($"If that fails, install manually: dotnet add package runtime.{rid}.Microsoft.NETCore.ILDAsm --version {PackageVersion}");
        return 1;
    }
}

var ildasmBinary = FindIldasmBinary(packageRoot, rid);

if (ildasmBinary is null)
{
    Console.Error.WriteLine($"ildasm binary not found in {packageRoot}.");
    return 1;
}

var runViaDotnet = ildasmBinary.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);

var psi = new ProcessStartInfo
{
    FileName = runViaDotnet ? "dotnet" : ildasmBinary,
    WorkingDirectory = Directory.GetCurrentDirectory(),
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false
};

if (runViaDotnet)
{
    psi.ArgumentList.Add(ildasmBinary);
}

foreach (var arg in args)
{
    psi.ArgumentList.Add(arg);
}

using var proc = Process.Start(psi);
await Task.WhenAll(
    proc!.StandardOutput.BaseStream.CopyToAsync(Console.OpenStandardOutput()),
    proc.StandardError.BaseStream.CopyToAsync(Console.OpenStandardError()));
proc.WaitForExit();
return proc.ExitCode;

static string? FindIldasmBinary(string packageRoot, string rid)
{
    // Check standard location: runtimes/{rid}/native/ildasm.exe
    var nativePath = Path.Combine(packageRoot, "runtimes", rid, "native", "ildasm.exe");
    if (File.Exists(nativePath))
    {
        return nativePath;
    }

    // Fallback: search for ildasm.exe or ildasm.dll anywhere
    foreach (var pattern in new[] { "ildasm.exe", "ildasm.dll", "ildasm" })
    {
        var match = Directory.EnumerateFiles(packageRoot, pattern, SearchOption.AllDirectories).FirstOrDefault();
        if (match is not null)
        {
            return match;
        }
    }

    return null;
}