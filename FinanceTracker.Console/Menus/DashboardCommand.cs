using FinanceTracker.Core.Interfaces;
using Spectre.Console;

namespace FinanceTracker.Console.Menus;

public class DashboardCommand
{
    private readonly IBalanceService _balanceService;
    private readonly IExpenseService _expenseService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IDocumentService _documentService;

    public DashboardCommand(
        IBalanceService balanceService,
        IExpenseService expenseService,
        ISubscriptionService subscriptionService,
        IDocumentService documentService)
    {
        _balanceService = balanceService;
        _expenseService = expenseService;
        _subscriptionService = subscriptionService;
        _documentService = documentService;
    }

    public async Task ShowAsync()
    {
        var now = DateTime.Today;
        var netWorth = await _balanceService.GetCurrentNetWorthAsync();
        var monthlySpending = await _expenseService.GetMonthlyTotalAsync(now.Year, now.Month);
        var monthlySubs = await _subscriptionService.GetMonthlyEstimateAsync();
        var upcoming = (await _subscriptionService.GetUpcomingAsync(7)).ToList();
        var expiring = (await _documentService.GetExpiringSoonAsync(30)).ToList();

        var grid = new Grid().AddColumn().AddColumn();
        grid.AddRow("[bold]Net Worth[/]", $"[green]{netWorth:C}[/]");
        grid.AddRow("[bold]Spending This Month[/]", $"{monthlySpending:C}");
        grid.AddRow("[bold]Monthly Subscriptions[/]", $"{monthlySubs:C}");
        grid.AddRow("[bold]Upcoming Subscriptions (7d)[/]", $"{upcoming.Count}");
        grid.AddRow("[bold]Documents Expiring (30d)[/]",
            expiring.Count > 0 ? $"[yellow]{expiring.Count}[/]" : "0");

        AnsiConsole.Write(new Panel(grid)
            .Header("[green]Dashboard Summary[/]")
            .Border(BoxBorder.Rounded));

        AnsiConsole.WriteLine();
    }
}
