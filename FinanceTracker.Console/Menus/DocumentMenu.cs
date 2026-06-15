using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Spectre.Console;

namespace FinanceTracker.Console.Menus;

public class DocumentMenu
{
    private readonly IDocumentService _documentService;

    public DocumentMenu(IDocumentService documentService) => _documentService = documentService;

    public async Task RunAsync()
    {
        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Documents[/]")
                    .AddChoices("View expiring documents", "Add document", "Back"));

            switch (choice)
            {
                case "View expiring documents":
                    await ViewExpiringAsync();
                    break;
                case "Add document":
                    await AddAsync();
                    break;
                case "Back":
                    return;
            }
        }
    }

    private async Task ViewExpiringAsync()
    {
        var expiring = (await _documentService.GetExpiringSoonAsync(30)).ToList();
        var expired = (await _documentService.GetExpiredAsync()).ToList();

        if (expired.Count == 0 && expiring.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No expired or soon-to-expire documents.[/]");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumns("Name", "Type", "Expiration", "Status");
        foreach (var d in expired)
        {
            table.AddRow(
                Markup.Escape(d.Name), d.Type.ToString(),
                d.ExpirationDate.ToString("yyyy-MM-dd"), "[red]Expired[/]");
        }
        foreach (var d in expiring)
        {
            table.AddRow(
                Markup.Escape(d.Name), d.Type.ToString(),
                d.ExpirationDate.ToString("yyyy-MM-dd"), "[yellow]Expiring Soon[/]");
        }

        AnsiConsole.Write(table);
    }

    private async Task AddAsync()
    {
        var name = AnsiConsole.Ask<string>("[green]Document name[/]:");
        var type = AnsiConsole.Prompt(
            new SelectionPrompt<DocumentType>()
                .Title("[green]Type[/]:")
                .AddChoices(Enum.GetValues<DocumentType>()));
        var expiration = AnsiConsole.Ask("[green]Expiration date[/] (yyyy-MM-dd):",
            DateTime.Today.AddYears(1));

        DateTime? reminder = null;
        if (AnsiConsole.Confirm("Set a reminder date?", false))
        {
            reminder = AnsiConsole.Ask("[green]Reminder date[/] (yyyy-MM-dd):",
                expiration.AddDays(-30));
        }

        var notes = AnsiConsole.Prompt(
            new TextPrompt<string>("Notes (optional):").AllowEmpty());

        await _documentService.AddAsync(new Document
        {
            Name = name,
            Type = type,
            ExpirationDate = expiration,
            ReminderDate = reminder,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
        });

        AnsiConsole.MarkupLine("[green]Document added.[/]");
    }
}
