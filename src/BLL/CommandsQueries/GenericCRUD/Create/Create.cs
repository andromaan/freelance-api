using AutoMapper;
using BLL.Common.Handlers;
using BLL.Common.Interfaces.Repositories;
using BLL.Services;
using Domain.Common.Abstractions;
using MediatR;

namespace BLL.CommandsQueries.GenericCRUD.Create;

public class Create
{
    public record Command<TCreateViewModel, TViewModel> : IRequest<ServiceResponse<TViewModel?>>
        where TCreateViewModel : class
        where TViewModel : class
    {
        public required TCreateViewModel Model { get; init; }
    }

    public class CommandHandler<TCreateViewModel, TViewModel, TEntity, TKey, TQueries>(
        IRepository<TEntity, TKey> repository,
        IMapper mapper,
        TQueries queries,
        IEnumerable<ICreateHandler<TEntity, TCreateViewModel, TViewModel>> handlers)
        : IRequestHandler<Command<TCreateViewModel, TViewModel>, ServiceResponse<TViewModel?>>
        where TEntity : Entity<TKey>
        where TCreateViewModel : class
        where TViewModel : class
        where TQueries : IQueries<TEntity, TKey>
    {
        public async Task<ServiceResponse<TViewModel?>> Handle(Command<TCreateViewModel, TViewModel> request,
            CancellationToken cancellationToken)
        {
            // 1. Map to entity
            var mappedEntity = mapper.Map<TEntity>(request.Model);

            // 2. Check uniqueness
            if (queries is IUniqueQuery<TEntity, TKey> uniqueQuery)
            {
                if (!await uniqueQuery.IsUniqueAsync(mappedEntity, cancellationToken))
                {
                    return ServiceResponse<TViewModel?>.BadRequest(
                        $"{typeof(TEntity).Name} with the same unique fields already exists");
                }
            }


            try
            {
                // 3. Execute new unified handlers (validation + processing in one place)
                foreach (var handler in handlers)
                {
                    var result = await handler.HandleAsync(
                        mappedEntity!,
                        request.Model,
                        cancellationToken);

                    if (result is { Success: false })
                    {
                        return result;
                    }
                }

                // 4. Save to database
                var createdEntity = await repository.CreateAsync(mappedEntity!, cancellationToken);
                return ServiceResponse<TViewModel?>.Ok($"{typeof(TEntity).Name} created",
                    mapper.Map<TViewModel>(createdEntity));
            }
            catch (Exception exception)
            {
                return ServiceResponse<TViewModel?>.InternalError(exception.Message);
            }
        }
    }
}