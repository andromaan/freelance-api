using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.Common.Interfaces.Repositories;
using BLL.Services;
using Domain.Common.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Extensions;

public static class CommandRegistrationExtensions
{
    public static IServiceCollection AddUpdateCommandHandler<TEntity, TKey, TViewModel, TUpdateViewModel, TQueries>(
        this IServiceCollection services, Type[]? specificUpdateVMs = null)
        where TEntity : Entity<TKey>
        where TViewModel : class
        where TUpdateViewModel : class
        where TQueries : class, IQueries<TEntity, TKey>
    {
        services.AddTransient(
            typeof(IRequestHandler<Update.Command<TUpdateViewModel, TKey, TViewModel>, Result<TViewModel?>>),
            typeof(Update.CommandHandler<TUpdateViewModel, TViewModel, TEntity, TKey, TQueries>)
        );

        if (specificUpdateVMs != null)
        {
            foreach (var updateVm in specificUpdateVMs)
            {
                var requestType = typeof(Update.Command<,,>).MakeGenericType(updateVm, typeof(TKey), typeof(TViewModel));
                var handlerType = typeof(Update.CommandHandler<,,,,>)
                    .MakeGenericType(updateVm, typeof(TViewModel), typeof(TEntity), typeof(TKey), typeof(TQueries));
                var responseType = typeof(Result<>).MakeGenericType(typeof(TViewModel));
                var serviceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

                services.AddTransient(serviceType, handlerType);
            }
        }

        return services;
    }

    public static IServiceCollection AddUpdateByUserCommandHandler<TEntity, TKey, TViewModel, TUpdateViewModel, TQueries>(
        this IServiceCollection services, Type[]? specificUpdateVMs = null)
        where TEntity : Entity<TKey>
        where TViewModel : class
        where TUpdateViewModel : class
        where TQueries : class, IQueries<TEntity, TKey>, IByUserQuery<TEntity, TKey>
    {
        services.AddTransient(
            typeof(IRequestHandler<UpdateByUser.Command<TUpdateViewModel, TViewModel>, Result<TViewModel?>>),
            typeof(UpdateByUser.CommandHandler<TUpdateViewModel, TViewModel, TEntity, TKey, TQueries>)
        );

        if (specificUpdateVMs != null)
        {
            foreach (var updateVm in specificUpdateVMs)
            {
                var requestType = typeof(UpdateByUser.Command<,>).MakeGenericType(updateVm, typeof(TViewModel));
                var handlerType = typeof(UpdateByUser.CommandHandler<,,,,>)
                    .MakeGenericType(updateVm, typeof(TViewModel), typeof(TEntity), typeof(TKey), typeof(TQueries));
                var responseType = typeof(Result<>).MakeGenericType(typeof(TViewModel));
                var serviceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

                services.AddTransient(serviceType, handlerType);
            }
        }

        return services;
    }

    public static IServiceCollection AddCreateCommandHandler<TEntity, TKey, TViewModel, TCreateViewModel, TQueries>(
        this IServiceCollection services, Type[]? specificCreateVMs = null)
        where TEntity : Entity<TKey>
        where TViewModel : class
        where TCreateViewModel : class
        where TQueries : class, IQueries<TEntity, TKey>
    {
        services.AddTransient(
            typeof(IRequestHandler<Create.Command<TCreateViewModel, TViewModel>, Result<TViewModel?>>),
            typeof(Create.CommandHandler<TCreateViewModel, TViewModel, TEntity, TKey, TQueries>)
        );

        if (specificCreateVMs != null)
        {
            foreach (var createVm in specificCreateVMs)
            {
                var requestType = typeof(Create.Command<,>).MakeGenericType(createVm, typeof(TViewModel));
                var handlerType = typeof(Create.CommandHandler<,,,,>)
                    .MakeGenericType(createVm, typeof(TViewModel), typeof(TEntity), typeof(TKey), typeof(TQueries));
                var responseType = typeof(Result<>).MakeGenericType(typeof(TViewModel));
                var serviceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

                services.AddTransient(serviceType, handlerType);
            }
        }

        return services;
    }
}
