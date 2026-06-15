using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Spectre.Console;

namespace FinanceTracker.Console.Menus;

public class AccountMenu
{
    private readonly IAccountService _accountService;

    public AccountMenu(IAccountService accountService) => _accountService = accountService;

    public async Task RunAsync()
    {
        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Accounts[/]")
                    .AddChoices("View accounts", "Add account", "Back"));

            switch (choice)
            {
                case "View accounts":
                    await ViewAsync();
                    break;
                case "Add account":
                    await AddAsync();
                    break;
                case "Back":
                    return;
            }
        }
    }

    private async Task ViewAsync()
    {
        var accounts = (await _accountService.GetAllAccountsAsync()).ToList();
        if (accounts.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No accounts yet.[/]");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumns("Id", "Name", "Type", "Balance", "Institution", "Active");
        foreach (var a in accounts)
        {
            table.AddRow(
                a.Id.ToString(),
                Markup.Escape(a.Name),
                a.Type.ToString(),
                $"{a.Balance:C}",
                Markup.Escape(a.Institution ?? "-"),
                a.IsActive ? "[green]yes[/]" : "[grey]no[/]");
        }

        AnsiConsole.Write(table);

        var total = await _accountService.GetTotalBalanceAsync();
        AnsiConsole.MarkupLine($"[bold]Total balance (active):[/] [green]{total:C}[/]");
    }

    private async Task AddAsync()
    {
        var name = AnsiConsole.Ask<string>("Account [green]name[/]:");
        var type = AnsiConsole.Prompt(
            new SelectionPrompt<AccountType>()
                .Title("Account [green]type[/]:")
                .AddChoices(Enum.GetValues<AccountType>()));
        var balance = AnsiConsole.Ask<decimal>("Current [green]balance[/]:");
        var institution = AnsiConsole.Prompt(
            new TextPrompt<string>("Institution (optional):").AllowEmpty());
        var notes = AnsiConsole.Prompt(
            new TextPrompt<string>("Notes (optional):").AllowEmpty());

        await _accountService.AddAccountAsync(new Account
        {
            Name = name,
            Type = type,
            Balance = balance,
            Institution = string.IsNullOrWhiteSpace(institution) ? null : institution,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
        });

        AnsiConsole.MarkupLine("[green]Account added.[/]");
    }
}
