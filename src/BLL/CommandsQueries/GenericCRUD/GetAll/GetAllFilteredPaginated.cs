using AutoMapper;
using BLL.Common.Handlers;
using BLL.Common.Interfaces.Repositories;
using BLL.Services;
using BLL.ViewModels;
using Domain.Common.Abstractions;
using MediatR;

namespace BLL.CommandsQueries.GenericCRUD.GetAll;

public class GetAllFilteredPaginated
{
    public record Query<TFilteringModel, TViewModel>(PagedVM PagedVm, TFilteringModel FilteringVm)
        : IRequest<Result<PaginatedItemsVM<TViewModel>?>>
        where TFilteringModel : class
        where TViewModel : class;

    public class QueryHandler<TEntity, TKey, TViewModel, TQueries, TFilteringModel>(
        TQueries queries,
        IMapper mapper,
        IGetAllFilteredHandler<TEntity, TFilteringModel, TViewModel> handler)
        : IRequestHandler<Query<TFilteringModel, TViewModel>, Result<PaginatedItemsVM<TViewModel>?>>
        where TEntity : Entity<TKey>
        where TViewModel : class
        where TFilteringModel : class
        where TQueries : IQueries<TEntity, TKey>
    {
        public async Task<Result<PaginatedItemsVM<TViewModel>?>> Handle(Query<TFilteringModel, TViewModel> request,
            CancellationToken cancellationToken)
        {
            try
            {
                var entities = (await queries.GetAllAsync(cancellationToken)).ToList();

                var (result, filteredEntities) = await handler.HandleAsync(
                    entities,
                    request.FilteringVm,
                    cancellationToken);

                if (result is { Success: false })
                {
                    return result;
                }

                entities = filteredEntities!;
                int totalCount = entities.Count;

                var viewModels = mapper.Map<List<TViewModel>>(entities
                    .Skip((request.PagedVm.Page - 1) * request.PagedVm.PageSize)
                    .Take(request.PagedVm.PageSize));

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