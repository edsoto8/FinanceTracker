using FinanceTracker.Data.Database;

namespace FinanceTracker.Tests.Helpers;

public static class TestDatabase
{
    public static async Task<string> CreateAsync()
    {
        var path = Path.GetTempFileName();
        var cs = $"Data Source={path}";
        var init = new DatabaseInitializer(cs);
        await init.InitializeAsync();
        return cs;
    }
}
