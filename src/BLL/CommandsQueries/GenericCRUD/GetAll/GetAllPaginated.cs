using AutoMapper;
using BLL.Common.Interfaces.Repositories;
using BLL.Services;
using BLL.ViewModels;
using Domain.Common.Abstractions;
using MediatR;

namespace BLL.CommandsQueries.GenericCRUD.GetAll;

public class GetAllPaginated
{
    // ReSharper disable once UnusedTypeParameter
    public record Query<TViewModel>(PagedVM PagedVm) : IRequest<Result<PaginatedItemsVM<TViewModel>?>> where TViewModel : class;

    public class QueryHandler<TEntity, TKey, TViewModel, TQueries>(
        TQueries queries,
        IMapper mapper)
        : IRequestHandler<Query<TViewModel>, Result<PaginatedItemsVM<TViewModel>?>>
        where TEntity : Entity<TKey>
        where TViewModel : class
        where TQueries : IQueries<TEntity, TKey>
    {
        public async Task<Result<PaginatedItemsVM<TViewModel>?>> Handle(Query<TViewModel> request, CancellationToken cancellationToken)
        {
            try
            {
                var (entities, totalCount) = await queries.GetPaginatedAsync(request.PagedVm.Page,
                    request.PagedVm.PageSize,
                    cancellationToken);

                var viewModels = mapper.Map<List<TViewModel>>(entities);

                var pagedResponse = new PaginatedItemsVM<TViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    Page = request.PagedVm.Page,
                    PageSize = request.PagedVm.PageSize,
                    PageCount = (int)Math.Ceiling((double)totalCount / request.PagedVm.PageSize),
                };

                return Result<PaginatedItemsVM<TViewModel>?>.Ok($"{typeof(TEntity).Name}s retrieved", pagedResponse);
            }
            catch (Exception exception)
            {
                return Result<PaginatedItemsVM<TViewModel>?>.InternalError(exception.Message);
            }
        }
    }
}