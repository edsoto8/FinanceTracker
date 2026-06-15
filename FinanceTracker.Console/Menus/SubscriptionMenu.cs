using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Spectre.Console;

namespace FinanceTracker.Console.Menus;

public class SubscriptionMenu
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICategoryService _categoryService;
    private readonly IAccountService _accountService;

    public SubscriptionMenu(
        ISubscriptionService subscriptionService,
        ICategoryService categoryService,
        IAccountService accountService)
    {
        _subscriptionService = subscriptionService;
        _categoryService = categoryService;
        _accountService = accountService;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Subscriptions[/]")
                    .AddChoices("View active subscriptions", "Add subscription", "Back"));

            switch (choice)
            {
                case "View active subscriptions":
                    await ViewActiveAsync();
                    break;
                case "Add subscription":
                    await AddAsync();
                    break;
                case "Back":
                    return;
            }
        }
    }

    private async Task ViewActiveAsync()
    {
        var subscriptions = (await _subscriptionService.GetActiveAsync()).ToList();
        var estimate = await _subscriptionService.GetMonthlyEstimateAsync();

        AnsiConsole.MarkupLine($"[bold]Estimated monthly cost:[/] [green]{estimate:C}[/]");

        if (subscriptions.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No active subscriptions.[/]");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumns("Name", "Cost", "Frequency", "Next Billing");
        foreach (var s in subscriptions)
        {
            table.AddRow(
                Markup.Escape(s.Name),
                $"{s.Cost:C}",
                s.Frequency.ToString(),
                s.NextBillingDate.ToString("yyyy-MM-dd"));
        }

        AnsiConsole.Write(table);
    }

    private async Task AddAsync()
    {
        var name = AnsiConsole.Ask<string>("[green]Name[/]:");
        var cost = AnsiConsole.Ask<decimal>("[green]Cost[/]:");
        var frequency = AnsiConsole.Prompt(
            new SelectionPrompt<BillingFrequency>()
                .Title("[green]Billing frequency[/]:")
                .AddChoices(Enum.GetValues<BillingFrequency>()));
        var nextBilling = AnsiConsole.Ask("[green]Next billing date[/] (yyyy-MM-dd):", DateTime.Today);

        var categories = (await _categoryService.GetAllAsync()).ToList();
        var category = AnsiConsole.Prompt(
            new SelectionPrompt<Category>()
                .Title("[green]Category[/]:")
                .UseConverter(c => c.Name)
                .AddChoices(categories));

        var accounts = (await _accountService.GetActiveAccountsAsync()).ToList();
        int? accountId = null;
        if (accounts.Count > 0 && AnsiConsole.Confirm("Link to a payment account?", false))
        {
            var account = AnsiConsole.Prompt(
                new SelectionPrompt<Account>()
                    .Title("[green]Account[/]:")
                    .UseConverter(a => a.Name)
                    .AddChoices(accounts));
            accountId = account.Id;
        }

        await _subscriptionService.AddAsync(new Subscription
        {
            Name = name,
            Cost = cost,
            Frequency = frequency,
            NextBillingDate = nextBilling,
            CategoryId = category.Id,
            AccountId = accountId
        });

        AnsiConsole.MarkupLine("[green]Subscription added.[/]");
    }
}
