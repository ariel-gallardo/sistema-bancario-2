using System.Threading;
using System.Threading.Tasks;

namespace BankingApp.SharedKernel.Logging;

public interface ILoggingClient
{
    Task SendErrorAsync(ErrorLogEntry entry, CancellationToken cancellationToken = default);
}
