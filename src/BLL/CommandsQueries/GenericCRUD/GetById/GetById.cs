using AutoMapper;
using BLL.Common.Interfaces.Repositories;
using BLL.Services;
using Domain.Common.Abstractions;
using MediatR;

namespace BLL.CommandsQueries.GenericCRUD.GetById;

public class GetById
{
    // ReSharper disable once UnusedTypeParameter
    public record Query<TKey, TViewModel> : IRequest<Result<TViewModel?>> where TViewModel : class
    {
        public required TKey Id { get; init; }
    }

    public class QueryHandler<TEntity, TKey, TViewModel, TQueries>(TQueries queries, IMapper mapper)
        : IRequestHandler<Query<TKey, TViewModel>, Result<TViewModel?>>
        where TEntity : Entity<TKey>
        where TViewModel : class
        where TQueries : IQueries<TEntity, TKey>
    {
        public async Task<Result<TViewModel?>> Handle(Query<TKey, TViewModel> request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await queries.GetByIdAsync(request.Id, cancellationToken);
                if (entity == null)
                {
                    return Result<TViewModel?>.NotFound($"{typeof(TEntity).Name} not found");
                }
                var viewModel = mapper.Map<TViewModel>(entity);
                return Result<TViewModel?>.Ok($"{typeof(TEntity).Name} retrieved", viewModel);
            }
            catch (Exception exception)
            {
                return Result<TViewModel?>.InternalError(exception.Message);
            }
        }
    }
}