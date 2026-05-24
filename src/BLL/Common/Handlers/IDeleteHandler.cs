using BLL.Services;

namespace BLL.Common.Handlers;

/// <summary>
/// Unified handler for Delete operations that combines validation and processing logic.
/// Returns Result on validation/logic failure, or the processed entity on success.
/// </summary>
public interface IDeleteHandler<TEntity, TViewModel>
    where TEntity : class
    where TViewModel : class
{
    /// <summary>
    /// Handles validation and processing for entity creation.
    /// </summary>
    /// <param name="entity">The mapped entity before processing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// Result&lt;TEntity, Result&gt; where:
    /// - Success case contains the processed entity
    /// - Failure case contains Result with error details
    /// </returns>
    Task<Result<TViewModel?>> HandleAsync(
        TEntity entity,
        CancellationToken cancellationToken);
}
