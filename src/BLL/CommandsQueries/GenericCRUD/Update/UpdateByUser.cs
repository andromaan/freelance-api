using AutoMapper;
using BLL.Common.Handlers;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories;
using BLL.Services;
using Domain.Common.Abstractions;
using MediatR;

namespace BLL.CommandsQueries.GenericCRUD.Update;

public class UpdateByUser
{
    public record Command<TUpdateViewModel, TViewModel> : IRequest<Result<TViewModel?>>
        where TUpdateViewModel : class
        where TViewModel : class
    {
        public required TUpdateViewModel Model { get; init; }
    }

    public class CommandHandler<TUpdateViewModel, TViewModel, TEntity, TKey, TQueries>(
        IRepository<TEntity, TKey> repository,
        TQueries queries,
        IMapper mapper,
        IUserProvider userProvider,
        IEnumerable<IUpdateHandler<TEntity, TUpdateViewModel, TViewModel>> handlers)
        : IRequestHandler<Command<TUpdateViewModel, TViewModel>, Result<TViewModel?>>
        where TEntity : Entity<TKey>
        where TUpdateViewModel : class
        where TViewModel : class
        where TQueries : IQueries<TEntity, TKey>, IByUserQuery<TEntity, TKey>
    {
        public async Task<Result<TViewModel?>> Handle(
            Command<TUpdateViewModel, TViewModel> request,
            CancellationToken cancellationToken)
        {
            var userId = await userProvider.GetUserId();
            
            // 1. Check entity existence
            var existingEntity = await queries.GetByUser(userId, cancellationToken);

            if (existingEntity == null)
            {
                return Result<TViewModel?>.NotFound(
                    $"{typeof(TEntity).Name} not found by user id {userId}");
            }

            // 2. Mapping
            if (request.Model is not ISkipMapper)
            {
                mapper.Map(request.Model, existingEntity);
            }

            // 3. Execute new unified handlers (validation + processing in one place)
            foreach (var handler in handlers)
            {
                var result = await handler.HandleAsync(
                    existingEntity,
                    request.Model,
                    cancellationToken);

                if (result is { Success: false })
                {
                    return result;
                }
            }

            // 4. Save to database
            try
            {
                await repository.UpdateAsync(existingEntity, cancellationToken);
                return Result<TViewModel?>.Ok(
                    $"{typeof(TEntity).Name} updated",
                    mapper.Map<TViewModel>(existingEntity));
            }
            catch (Exception exception)
            {
                return Result<TViewModel?>.InternalError(exception.Message);
            }
        }
    }
}