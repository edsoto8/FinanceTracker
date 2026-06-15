using Microsoft.Extensions.Configuration;

namespace FinanceTracker.Data;

public static class DatabasePathResolver
{
    /// <summary>
    /// Resolves the configured database file name to an absolute path under the
    /// user's local application data directory, creating the folder if needed.
    /// </summary>
    public static string Resolve(IConfiguration configuration)
    {
        var configured = configuration["Database:Path"];
        if (string.IsNullOrWhiteSpace(configured))
        {
            configured = "financetracker.db";
        }

        if (Path.IsPathRooted(configured))
        {
            var rootedDir = Path.GetDirectoryName(configured);
            if (!string.IsNullOrEmpty(rootedDir))
            {
                Directory.CreateDirectory(rootedDir);
            }

            return configured;
        }

        var dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FinanceTracker");
        Directory.CreateDirectory(dataDir);
        return Path.Combine(dataDir, configured);
    }
}
