using AutoMapper;
using BLL.Common.Interfaces.Repositories;
using BLL.Services;
using Domain.Common.Abstractions;
using MediatR;

namespace BLL.CommandsQueries.GenericCRUD.GetAll;

public class GetAll
{
    // ReSharper disable once UnusedTypeParameter
    public record Query<TViewModel> : IRequest<ServiceResponse<List<TViewModel>?>> where TViewModel : class;

    public class QueryHandler<TEntity, TKey, TViewModel, TQueries>(TQueries queries, IMapper mapper)
        : IRequestHandler<Query<TViewModel>, ServiceResponse<List<TViewModel>?>>
        where TEntity : Entity<TKey>
        where TViewModel : class
        where TQueries : IQueries<TEntity, TKey>
    {
        public async Task<ServiceResponse<List<TViewModel>?>> Handle(Query<TViewModel> request, CancellationToken cancellationToken)
        {
            try
            {
                var entities = await queries.GetAllAsync(cancellationToken);
                var viewModels = mapper.Map<List<TViewModel>>(entities);
                return ServiceResponse<List<TViewModel>?>.Ok($"{typeof(TEntity).Name}s retrieved", viewModels);
            }
            catch (Exception exception)
            {
                return ServiceResponse<List<TViewModel>?>.InternalError(exception.Message);
            }
        }
    }
}