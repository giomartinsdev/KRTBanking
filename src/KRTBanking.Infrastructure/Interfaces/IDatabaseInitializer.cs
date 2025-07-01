namespace KRTBanking.Infrastructure.Interfaces;

/// <summary>
/// Contract for database initialization operations.
/// Follows DIP by providing abstraction for infrastructure concerns.
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Initializes required database tables and structures.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Task representing the async operation</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
