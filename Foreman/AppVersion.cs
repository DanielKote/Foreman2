using System.Reflection;

namespace Foreman;

/// <summary>Application release version from <c>VersionPrefix</c> / <c>VersionSuffix</c> in Foreman.csproj.</summary>
internal static class AppVersion {
    /// <summary>SemVer string (e.g. 2.2.16 or 2.2.16-beta.1+build.42).</summary>
    public static string SemVer { get; } = ResolveSemVer();

    public static string Display => "v" + SemVer;

    public static string ProductName => "Foreman " + SemVer;

    private static string ResolveSemVer() {
        var informational = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
        if (!string.IsNullOrEmpty(informational))
            return informational;

        var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        return assemblyVersion is null
            ? "0.0.0"
            : assemblyVersion.Revision > 0
            ? $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}.{assemblyVersion.Revision}"
            : $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
    }
}
