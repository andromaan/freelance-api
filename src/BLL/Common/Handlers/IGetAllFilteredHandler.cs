using BLL.Services;
using BLL.ViewModels;

namespace BLL.Common.Handlers;

public interface IGetAllFilteredHandler<TEntity, in TFilterViewModel, TViewModel>
    where TEntity : class
    where TFilterViewModel : class
    where TViewModel : class
{
    Task<(ServiceResponse<PaginatedItemsVM<TViewModel>?> response, List<TEntity>? filteredEntities)> HandleAsync(
        List<TEntity> entities,
        TFilterViewModel filter,
        CancellationToken cancellationToken);
}