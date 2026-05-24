using BLL.Services;

namespace BLL.Common.Handlers;

/// <summary>
/// Unified handler for Create operations that combines validation and processing logic.
/// Returns Result on validation/logic failure, or the processed entity on success.
/// </summary>
public interface ICreateHandler<TEntity, TCreateViewModel, TViewModel>
    where TEntity : class
    where TCreateViewModel : class
    where TViewModel : class
{
    /// <summary>
    /// Handles validation and processing for entity creation.
    /// </summary>
    /// <param name="entity">The mapped entity before processing</param>
    /// <param name="createModel">The create view model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// Result&lt;TEntity, Result&gt; where:
    /// - Success case contains the processed entity
    /// - Failure case contains Result with error details
    /// </returns>
    Task<Result<TViewModel?>> HandleAsync(
        TEntity entity,
        TCreateViewModel createModel,
        CancellationToken cancellationToken);
}
