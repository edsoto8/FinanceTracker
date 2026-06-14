using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using FinanceTracker.Core.Services;
using FinanceTracker.Data.Database;
using FinanceTracker.Data.Repositories;
using Serilog;
using Spectre.Console;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/console-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("Starting FinanceTracker.Console");

const string connectionString = "Data Source=financetracker.db";

var dbInit = new DatabaseInitializer(connectionString);
await dbInit.InitializeAsync();

IAccountRepository accountRepo = new AccountRepository(connectionString);
IBalanceEntryRepository balanceRepo = new BalanceEntryRepository(connectionString);
IExpenseRepository expenseRepo = new ExpenseRepository(connectionString);
ISubscriptionRepository subscriptionRepo = new SubscriptionRepository(connectionString);
IDocumentRepository documentRepo = new DocumentRepository(connectionString);
ICategoryRepository categoryRepo = new CategoryRepository(connectionString);

var accountService = new AccountService(accountRepo, balanceRepo);
var expenseService = new ExpenseService(expenseRepo);
var subscriptionService = new SubscriptionService(subscriptionRepo);
var documentService = new DocumentService(documentRepo);

while (true)
{
    AnsiConsole.Clear();
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[bold green]Finance Tracker[/]")
            .AddChoices(
                "Dashboard",
                "Add Account", "View Accounts",
                "Add Expense", "View Expenses",
                "Add Subscription", "View Subscriptions",
                "Add Document", "View Documents",
                "Exit"));

    switch (choice)
    {
        case "Dashboard": await ShowDashboardAsync(); break;
        case "Add Account": await AddAccountAsync(); break;
        case "View Accounts": await ViewAccountsAsync(); break;
        case "Add Expense": await AddExpenseAsync(); break;
        case "View Expenses": await ViewExpensesAsync(); break;
        case "Add Subscription": await AddSubscriptionAsync(); break;
        case "View Subscriptions": await ViewSubscriptionsAsync(); break;
        case "Add Document": await AddDocumentAsync(); break;
        case "View Documents": await ViewDocumentsAsync(); break;
        case "Exit":
            Log.CloseAndFlush();
            return;
    }

    AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
    Console.ReadKey(true);
}

async Task ShowDashboardAsync()
{
    var now = DateTime.Now;
    var totalBalance = await accountService.GetTotalBalanceAsync();
    var monthlySpending = await expenseService.GetMonthlyTotalAsync(now.Year, now.Month);
    var monthlySubscriptions = await subscriptionService.GetEstimatedMonthlyTotalAsync();

    var panel = new Panel(
        $"[bold]Total Balance:[/] [green]{totalBalance:C}[/]\n" +
        $"[bold]Monthly Spending:[/] [red]{monthlySpending:C}[/]\n" +
        $"[bold]Estimated Monthly Subscriptions:[/] [yellow]{monthlySubscriptions:C}[/]")
        .Header("[bold]Summary[/]");
    AnsiConsole.Write(panel);

    var upcoming = (await subscriptionService.GetUpcomingAsync(30)).ToList();
    if (upcoming.Any())
    {
        AnsiConsole.MarkupLine("\n[bold]Upcoming Subscriptions (30 days):[/]");
        var t = new Table().AddColumn("Name").AddColumn("Due").AddColumn("Cost");
        foreach (var s in upcoming)
            t.AddRow(s.Name, s.NextBillingDate.ToString("MMM d"), s.Cost.ToString("C"));
        AnsiConsole.Write(t);
    }

    var expiring = (await documentService.GetExpiringSoonAsync(30)).ToList();
    if (expiring.Any())
    {
        AnsiConsole.MarkupLine("\n[bold]Documents Expiring Soon:[/]");
        var t = new Table().AddColumn("Name").AddColumn("Type").AddColumn("Expires");
        foreach (var d in expiring)
            t.AddRow(d.Name, d.Type.ToString(), d.ExpirationDate.ToString("MMM d, yyyy"));
        AnsiConsole.Write(t);
    }
}

async Task AddAccountAsync()
{
    var name = AnsiConsole.Ask<string>("Account name:");
    var institution = AnsiConsole.Ask<string>("Institution:");
    var type = AnsiConsole.Prompt(new SelectionPrompt<AccountType>().Title("Type:").AddChoices(Enum.GetValues<AccountType>()));
    var balance = AnsiConsole.Ask<decimal>("Current balance:");
    var notes = AnsiConsole.Ask<string>("Notes (optional):", "");

    var account = new Account { Name = name, Institution = institution, Type = type, Balance = balance, Notes = string.IsNullOrWhiteSpace(notes) ? null : notes };
    await accountService.AddAccountAsync(account);
    Log.Information("Added account {Name}", name);
    AnsiConsole.MarkupLine("[green]Account added.[/]");
}

async Task ViewAccountsAsync()
{
    var accounts = (await accountService.GetActiveAccountsAsync()).ToList();
    var t = new Table().AddColumn("Name").AddColumn("Institution").AddColumn("Type").AddColumn("Balance");
    foreach (var a in accounts)
        t.AddRow(a.Name, a.Institution, a.Type.ToString(), a.Balance.ToString("C"));
    AnsiConsole.Write(t);
    var total = accounts.Sum(a => a.Balance);
    AnsiConsole.MarkupLine($"[bold]Total: {total:C}[/]");
}

async Task AddExpenseAsync()
{
    var accounts = (await accountService.GetActiveAccountsAsync()).ToList();
    if (!accounts.Any()) { AnsiConsole.MarkupLine("[red]No accounts. Add an account first.[/]"); return; }

    var categories = (await categoryRepo.GetAllAsync()).Select(c => c.Name).ToList();
    var desc = AnsiConsole.Ask<string>("Description:");
    var amount = AnsiConsole.Ask<decimal>("Amount:");
    var date = AnsiConsole.Ask<DateTime>("Date (yyyy-MM-dd):", DateTime.Today);
    var category = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Category:").AddChoices(categories));
    var account = AnsiConsole.Prompt(new SelectionPrompt<Account>().Title("Account:").AddChoices(accounts).UseConverter(a => a.Name));

    var expense = new Expense { AccountId = account.Id, Category = category, Description = desc, Amount = amount, TransactionDate = date };
    await expenseService.AddExpenseAsync(expense);
    Log.Information("Added expense {Description} {Amount}", desc, amount);
    AnsiConsole.MarkupLine("[green]Expense added.[/]");
}

async Task ViewExpensesAsync()
{
    var from = AnsiConsole.Ask<DateTime>("From (yyyy-MM-dd):", DateTime.Today.AddMonths(-1));
    var to = AnsiConsole.Ask<DateTime>("To (yyyy-MM-dd):", DateTime.Today);
    var expenses = (await expenseService.GetByDateRangeAsync(from, to)).ToList();
    var t = new Table().AddColumn("Date").AddColumn("Description").AddColumn("Category").AddColumn("Amount");
    foreach (var e in expenses)
        t.AddRow(e.TransactionDate.ToString("MMM d"), e.Description, e.Category, e.Amount.ToString("C"));
    AnsiConsole.Write(t);
    AnsiConsole.MarkupLine($"[bold]Total: {expenses.Sum(e => e.Amount):C}[/]");
}

async Task AddSubscriptionAsync()
{
    var name = AnsiConsole.Ask<string>("Subscription name:");
    var cost = AnsiConsole.Ask<decimal>("Cost:");
    var freq = AnsiConsole.Prompt(new SelectionPrompt<BillingFrequency>().Title("Billing frequency:").AddChoices(Enum.GetValues<BillingFrequency>()));
    var nextDate = AnsiConsole.Ask<DateTime>("Next billing date (yyyy-MM-dd):", DateTime.Today.AddMonths(1));
    var category = AnsiConsole.Ask<string>("Category (optional):", "");

    var sub = new Subscription { Name = name, Cost = cost, Frequency = freq, NextBillingDate = nextDate, Category = string.IsNullOrWhiteSpace(category) ? null : category };
    await subscriptionService.AddSubscriptionAsync(sub);
    Log.Information("Added subscription {Name}", name);
    AnsiConsole.MarkupLine("[green]Subscription added.[/]");
}

async Task ViewSubscriptionsAsync()
{
    var subs = (await subscriptionService.GetActiveAsync()).ToList();
    var t = new Table().AddColumn("Name").AddColumn("Category").AddColumn("Frequency").AddColumn("Next Billing").AddColumn("Cost").AddColumn("Monthly");
    foreach (var s in subs)
        t.AddRow(s.Name, s.Category ?? "", s.Frequency.ToString(), s.NextBillingDate.ToString("MMM d, yyyy"), s.Cost.ToString("C"), s.MonthlyCost.ToString("C"));
    AnsiConsole.Write(t);
    AnsiConsole.MarkupLine($"[bold]Estimated monthly total: {subs.Sum(s => s.MonthlyCost):C}[/]");
}

async Task AddDocumentAsync()
{
    var name = AnsiConsole.Ask<string>("Document name:");
    var type = AnsiConsole.Prompt(new SelectionPrompt<DocumentType>().Title("Type:").AddChoices(Enum.GetValues<DocumentType>()));
    var expDate = AnsiConsole.Ask<DateTime>("Expiration date (yyyy-MM-dd):");
    var hasReminder = AnsiConsole.Confirm("Set reminder date?", false);
    DateTime? reminderDate = null;
    if (hasReminder)
        reminderDate = AnsiConsole.Ask<DateTime>("Reminder date (yyyy-MM-dd):", expDate.AddDays(-30));

    var doc = new Document { Name = name, Type = type, ExpirationDate = expDate, ReminderDate = reminderDate };
    await documentService.AddDocumentAsync(doc);
    Log.Information("Added document {Name}", name);
    AnsiConsole.MarkupLine("[green]Document added.[/]");
}

async Task ViewDocumentsAsync()
{
    var upcoming = await AnsiConsole.Status().StartAsync("Loading...", async _ =>
        (await documentService.GetExpiringSoonAsync(90)).ToList());

    var t = new Table().AddColumn("Name").AddColumn("Type").AddColumn("Expires").AddColumn("Reminder");
    foreach (var d in upcoming)
        t.AddRow(d.Name, d.Type.ToString(), d.ExpirationDate.ToString("MMM d, yyyy"), d.ReminderDate.HasValue ? d.ReminderDate.Value.ToString("MMM d, yyyy") : "-");
    AnsiConsole.Write(t.Title("Expiring within 90 days"));
}
