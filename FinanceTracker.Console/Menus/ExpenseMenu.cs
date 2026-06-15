using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Spectre.Console;

namespace FinanceTracker.Console.Menus;

public class ExpenseMenu
{
    private readonly IExpenseService _expenseService;
    private readonly ICategoryService _categoryService;
    private readonly IAccountService _accountService;

    public ExpenseMenu(
        IExpenseService expenseService,
        ICategoryService categoryService,
        IAccountService accountService)
    {
        _expenseService = expenseService;
        _categoryService = categoryService;
        _accountService = accountService;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Expenses[/]")
                    .AddChoices("View recent expenses", "Add expense", "Back"));

            switch (choice)
            {
                case "View recent expenses":
                    await ViewRecentAsync();
                    break;
                case "Add expense":
                    await AddAsync();
                    break;
                case "Back":
                    return;
            }
        }
    }

    private async Task ViewRecentAsync()
    {
        var expenses = (await _expenseService.GetRecentAsync(10)).ToList();
        if (expenses.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No expenses yet.[/]");
            return;
        }

        var categories = (await _categoryService.GetAllAsync())
            .ToDictionary(c => c.Id, c => c.Name);

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumns("Date", "Description", "Category", "Amount");
        foreach (var e in expenses)
        {
            table.AddRow(
                e.TransactionDate.ToString("yyyy-MM-dd"),
                Markup.Escape(e.Description),
                Markup.Escape(categories.TryGetValue(e.CategoryId, out var n) ? n : "-"),
                $"{e.Amount:C}");
        }

        AnsiConsole.Write(table);
    }

    private async Task AddAsync()
    {
        var description = AnsiConsole.Ask<string>("[green]Description[/]:");
        var amount = AnsiConsole.Ask<decimal>("[green]Amount[/]:");

        var categories = (await _categoryService.GetAllAsync()).ToList();
        var category = AnsiConsole.Prompt(
            new SelectionPrompt<Category>()
                .Title("[green]Category[/]:")
                .UseConverter(c => c.Name)
                .AddChoices(categories));

        var date = AnsiConsole.Ask("[green]Transaction date[/] (yyyy-MM-dd):", DateTime.Today);

        var accounts = (await _accountService.GetActiveAccountsAsync()).ToList();
        int? accountId = null;
        if (accounts.Count > 0 && AnsiConsole.Confirm("Link to an account?", false))
        {
            var account = AnsiConsole.Prompt(
                new SelectionPrompt<Account>()
                    .Title("[green]Account[/]:")
                    .UseConverter(a => a.Name)
                    .AddChoices(accounts));
            accountId = account.Id;
        }

        var paymentMethod = AnsiConsole.Prompt(
            new TextPrompt<string>("Payment method (optional):").AllowEmpty());

        await _expenseService.AddExpenseAsync(new Expense
        {
            Description = description,
            Amount = amount,
            CategoryId = category.Id,
            TransactionDate = date,
            AccountId = accountId,
            PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? null : paymentMethod
        });

        AnsiConsole.MarkupLine("[green]Expense added.[/]");
    }
}
