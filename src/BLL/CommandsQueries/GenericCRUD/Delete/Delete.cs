using BLL.Common.Handlers;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories;
using BLL.Services;
using Domain.Common.Abstractions;
using MediatR;

namespace BLL.CommandsQueries.GenericCRUD.Delete;

public class Delete
{
    // ReSharper disable once UnusedTypeParameter
    public record Command<TViewModel, TKey> : IRequest<ServiceResponse<TViewModel?>> where TViewModel : class
    {
        public required TKey Id { get; init; }
    }

    public class CommandHandler<TViewModel, TEntity, TKey>(
        IRepository<TEntity, TKey> repository,
        IQueries<TEntity, TKey> queries,
        IUserProvider userProvider,
        IEnumerable<IDeleteHandler<TEntity, TViewModel>> handlers)
        : IRequestHandler<Command<TViewModel, TKey>, ServiceResponse<TViewModel?>>
        where TEntity : Entity<TKey>
        where TViewModel : class
    {
        public async Task<ServiceResponse<TViewModel?>> Handle(Command<TViewModel, TKey> request,
            CancellationToken cancellationToken)
        {
            var existingEntity = await queries.GetByIdAsync(request.Id, cancellationToken);

            if (existingEntity is null)
            {
                return ServiceResponse<TViewModel?>.NotFound($"{typeof(TEntity).Name} with ID {request.Id} not found");
            }

            if (existingEntity is AuditableEntity<TKey> auditable)
            {
                var userId = await userProvider.GetUserId();
                var userRole = userProvider.GetUserRole();

                if (auditable.CreatedBy != userId && userRole != Settings.Roles.AdminRole &&
                    userRole != Settings.Roles.ModeratorRole)
                {
                    return ServiceResponse<TViewModel?>.Forbidden("You do not have permission to delete this entity");
                }
            }
            
            foreach (var handler in handlers)
            {
                var result = await handler.HandleAsync(
                    existingEntity,
                    cancellationToken);

                if (result is { Success: false })
                {
                    return result;
                }
            }

            try
            {
                await repository.DeleteAsync(request.Id, cancellationToken);
                return ServiceResponse<TViewModel?>.Ok($"{typeof(TEntity).Name} deleted");
            }
            catch (Exception exception)
            {
                return ServiceResponse<TViewModel?>.InternalError(exception.Message);
            }
        }
    }
}