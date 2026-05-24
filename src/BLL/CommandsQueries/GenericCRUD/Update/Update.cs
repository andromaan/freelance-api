using AutoMapper;
using BLL.Common.Handlers;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories;
using BLL.Services;
using Domain.Common.Abstractions;
using MediatR;

namespace BLL.CommandsQueries.GenericCRUD.Update;

public class Update
{
    public record Command<TUpdateViewModel, TKey, TViewModel> : IRequest<Result<TViewModel?>>
        where TUpdateViewModel : class
        where TViewModel : class
    {
        public required TKey Id { get; init; }
        public required TUpdateViewModel Model { get; init; }
    }

    public class CommandHandler<TUpdateViewModel, TViewModel, TEntity, TKey, TQueries>(
        IRepository<TEntity, TKey> repository,
        TQueries queries,
        IMapper mapper,
        IUserProvider userProvider,
        IEnumerable<IUpdateHandler<TEntity, TUpdateViewModel, TViewModel>> handlers)
        : IRequestHandler<Command<TUpdateViewModel, TKey, TViewModel>, Result<TViewModel?>>
        where TEntity : Entity<TKey>
        where TUpdateViewModel : class
        where TViewModel : class
        where TQueries : IQueries<TEntity, TKey>
    {
        public async Task<Result<TViewModel?>> Handle(
            Command<TUpdateViewModel, TKey, TViewModel> request,
            CancellationToken cancellationToken)
        {
            // 1. Check entity existence
            var existingEntity = await queries.GetByIdAsync(request.Id, cancellationToken);

            if (existingEntity == null)
            {
                return Result<TViewModel?>.NotFound(
                    $"{typeof(TEntity).Name} with ID {request.Id} not found");
            }

            // 2. Check access rights (auditable)
            if (request.Model is not ISkipAuditable)
            {
                if (existingEntity is AuditableEntity<TKey> auditable)
                {
                    var userId = await userProvider.GetUserId();
                    var userRole = userProvider.GetUserRole();

                    if (auditable.CreatedBy != userId && userRole != Settings.Roles.AdminRole &&
                        userRole != Settings.Roles.ModeratorRole)
                    {
                        return Result<TViewModel?>.Forbidden(
                            "You do not have permission to edit this entity");
                    }
                }
            }

            // 3. Mapping
            if (request.Model is not ISkipMapper)
            {
                mapper.Map(request.Model, existingEntity);
            }
            
            // 4. Check uniqueness
            if (queries is IUniqueQuery<TEntity, TKey> uniqueQuery)
            {
                if (!await uniqueQuery.IsUniqueAsync(existingEntity, cancellationToken))
                {
                    return Result<TViewModel?>.BadRequest(
                        $"{typeof(TEntity).Name} with the same unique fields already exists");
                }
            }

            // 5. Execute new unified handlers (validation + processing in one place)
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

            // 6. Save to database
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