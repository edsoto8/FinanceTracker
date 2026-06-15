using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Core.Services;

public class BalanceService : IBalanceService
{
    private readonly IBalanceEntryRepository _balanceRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<BalanceService> _logger;

    public BalanceService(
        IBalanceEntryRepository balanceRepository,
        IAccountRepository accountRepository,
        ILogger<BalanceService> logger)
    {
        _balanceRepository = balanceRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<BalanceEntry>> GetHistoryAsync(int accountId)
    {
        var entries = await _balanceRepository.GetByAccountIdAsync(accountId);
        return entries.OrderBy(e => e.EntryDate);
    }

    public async Task<BalanceEntry> AddEntryAsync(BalanceEntry entry)
    {
        entry.CreatedAt = DateTime.Now;
        entry.Id = await _balanceRepository.InsertAsync(entry);
        _logger.LogInformation(
            "Balance entry added for account {AccountId}: {Amount}", entry.AccountId, entry.Amount);
        return entry;
    }

    public async Task<decimal> GetCurrentNetWorthAsync()
    {
        var accounts = await _accountRepository.GetAllAsync();
        decimal netWorth = 0m;

        foreach (var account in accounts.Where(a => a.IsActive))
        {
            var entries = await _balanceRepository.GetByAccountIdAsync(account.Id);
            var latest = entries.MaxBy(e => e.EntryDate);
            netWorth += latest?.Amount ?? account.Balance;
        }

        return netWorth;
    }
}
