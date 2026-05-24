using BLL.Services;

namespace BLL.Common.Handlers;

/// <summary>
/// Unified handler for Update operations that combines validation and processing logic.
/// Returns Result on validation/logic failure, or the processed entity on success.
/// </summary>
public interface IUpdateHandler<TEntity, TUpdateViewModel, TViewModel>
    where TEntity : class
    where TUpdateViewModel : class
    where TViewModel : class
{
    /// <summary>
    /// Handles validation and processing for entity update.
    /// </summary>
    /// <param name="existingEntity">The existing entity from database</param>
    /// <param name="updateModel">The update view model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// Result&lt;TEntity, Result&gt; where:
    /// - Success case contains the processed entity
    /// - Failure case contains Result with error details
    /// </returns>
    Task<Result<TViewModel?>> HandleAsync(
        TEntity existingEntity,
        TUpdateViewModel updateModel,
        CancellationToken cancellationToken);
}
