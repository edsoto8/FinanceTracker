using Spectre.Console;

namespace FinanceTracker.Console.Menus;

public class MainMenu
{
    private readonly DashboardCommand _dashboard;
    private readonly AccountMenu _accounts;
    private readonly ExpenseMenu _expenses;
    private readonly SubscriptionMenu _subscriptions;
    private readonly DocumentMenu _documents;

    public MainMenu(
        DashboardCommand dashboard,
        AccountMenu accounts,
        ExpenseMenu expenses,
        SubscriptionMenu subscriptions,
        DocumentMenu documents)
    {
        _dashboard = dashboard;
        _accounts = accounts;
        _expenses = expenses;
        _subscriptions = subscriptions;
        _documents = documents;
    }

    public async Task RunAsync()
    {
        AnsiConsole.Write(new FigletText("Finance Tracker").Color(Color.Green));

        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Main Menu[/] — choose an option:")
                    .PageSize(10)
                    .AddChoices(
                        "Dashboard Summary",
                        "Accounts",
                        "Expenses",
                        "Subscriptions",
                        "Documents",
                        "Exit"));

            switch (choice)
            {
                case "Dashboard Summary":
                    await _dashboard.ShowAsync();
                    break;
                case "Accounts":
                    await _accounts.RunAsync();
                    break;
                case "Expenses":
                    await _expenses.RunAsync();
                    break;
                case "Subscriptions":
                    await _subscriptions.RunAsync();
                    break;
                case "Documents":
                    await _documents.RunAsync();
                    break;
                case "Exit":
                    AnsiConsole.MarkupLine("[grey]Goodbye.[/]");
                    return;
            }
        }
    }
}
