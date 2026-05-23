using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.CommandsQueries.GenericCRUD.Delete;
using BLL.CommandsQueries.GenericCRUD.GetAll;
using BLL.CommandsQueries.GenericCRUD.GetById;
using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.Common.Interfaces.Repositories;
using BLL.Services;
using BLL.ViewModels;
using Domain.Common.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Extensions;

public static class CrudRegistrationExtensions
{
    public static void RegisterCrudHandlers<TEntity, TKey, TQueries>(
        this IServiceCollection services, CrudRegistration<TEntity, TKey, TQueries> reg,
        Type[]? specificCreateVMs = null,
        Type[]? specificUpdateVMs = null)
        where TEntity : Entity<TKey>
        where TQueries : IQueries<TEntity, TKey>
    {
        var viewModelType = reg.ViewModelType;
        var entityType = reg.EntityType;
        var keyType = reg.KeyType;
        var queriesType = reg.QueriesInterfaceType;

        var responseListType = typeof(ServiceResponse<>).MakeGenericType(
            typeof(List<>).MakeGenericType(viewModelType));
        var responsePaginatedType = typeof(ServiceResponse<>).MakeGenericType(
            typeof(PaginatedItemsVM<>).MakeGenericType(viewModelType));
        var responseSingleType = typeof(ServiceResponse<>).MakeGenericType(viewModelType);

        var handlers = new List<HandlerDescriptor>
        {
            new HandlerDescriptor(
                typeof(GetAllPaginated.Query<>),
                typeof(GetAllPaginated.QueryHandler<,,,>),
                responsePaginatedType,
                [viewModelType],
                [
                    entityType,
                    keyType,
                    viewModelType,
                    queriesType
                ]),

            new HandlerDescriptor(
                typeof(GetAll.Query<>),
                typeof(GetAll.QueryHandler<,,,>),
                responseListType,
                [viewModelType],
                [
                    entityType,
                    keyType,
                    viewModelType,
                    queriesType
                ]),

            new HandlerDescriptor(
                typeof(GetById.Query<,>),
                typeof(GetById.QueryHandler<,,,>),
                responseSingleType,
                [keyType, viewModelType],
                [
                    entityType,
                    keyType,
                    viewModelType,
                    queriesType
                ]),

            new HandlerDescriptor(
                typeof(Delete.Command<,>),
                typeof(Delete.CommandHandler<,,>),
                responseSingleType,
                [viewModelType, keyType],
                [viewModelType, entityType, keyType])
        };

        if (reg.CreateViewModelType != null)
        {
            handlers.Add(new HandlerDescriptor(
                typeof(Create.Command<,>),
                typeof(Create.CommandHandler<,,,,>),
                responseSingleType,
                [reg.CreateViewModelType, viewModelType],
                [
                    reg.CreateViewModelType,
                    viewModelType,
                    entityType,
                    keyType,
                    queriesType
                ]));
        }

        if (reg.UpdateViewModelType != null)
        {
            handlers.Add(new HandlerDescriptor(
                typeof(Update.Command<,,>),
                typeof(Update.CommandHandler<,,,,>),
                responseSingleType,
                [reg.UpdateViewModelType, keyType, viewModelType],
                [
                    reg.UpdateViewModelType,
                    viewModelType,
                    entityType,
                    keyType,
                    queriesType
                ]));
        }

        if (reg.FilteringViewModelType != null)
        {
            var filteringResponseType = typeof(ServiceResponse<>).MakeGenericType(
                typeof(PaginatedItemsVM<>).MakeGenericType(viewModelType));

            handlers.Add(new HandlerDescriptor(
                typeof(GetAllFilteredPaginated.Query<,>),
                typeof(GetAllFilteredPaginated.QueryHandler<,,,,>),
                filteringResponseType,
                [reg.FilteringViewModelType, viewModelType],
                [
                    entityType,
                    keyType,
                    viewModelType,
                    queriesType,
                    reg.FilteringViewModelType
                ]));
        }

        foreach (var handler in handlers)
        {
            RegisterHandler(services, handler);
        }

        // Register additional create handlers
        if (specificCreateVMs != null)
        {
            foreach (var createVm in specificCreateVMs)
            {
                var createHandler = new HandlerDescriptor(
                    typeof(Create.Command<,>),
                    typeof(Create.CommandHandler<,,,,>),
                    responseSingleType,
                    [createVm, viewModelType],
                    [
                        createVm,
                        viewModelType,
                        entityType,
                        keyType,
                        queriesType
                    ]);
                RegisterHandler(services, createHandler);
            }
        }

        // Register additional update handlers
        if (specificUpdateVMs != null)
        {
            foreach (var updateVm in specificUpdateVMs)
            {
                var updateHandler = new HandlerDescriptor(
                    typeof(Update.Command<,,>),
                    typeof(Update.CommandHandler<,,,,>),
                    responseSingleType,
                    [updateVm, keyType, viewModelType],
                    [
                        updateVm,
                        viewModelType,
                        entityType,
                        keyType,
                        queriesType
                    ]);
                RegisterHandler(services, updateHandler);
            }
        }
    }

    private static void RegisterHandler(IServiceCollection services, HandlerDescriptor descriptor)
    {
        var requestType = descriptor.RequestType.MakeGenericType(descriptor.RequestTypeArgs);
        var handlerType = descriptor.HandlerType.MakeGenericType(descriptor.HandlerTypeArgs);
        var serviceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, descriptor.ResponseType);

        services.AddTransient(serviceType, handlerType);
    }

    private record HandlerDescriptor(
        Type RequestType,
        Type HandlerType,
        Type ResponseType,
        Type[] RequestTypeArgs,
        Type[] HandlerTypeArgs);
}

public class CrudRegistration<TEntity, TKey, TQueries>
    where TEntity : Entity<TKey>
    where TQueries : IQueries<TEntity, TKey>
{
    public Type EntityType => typeof(TEntity);
    public Type KeyType => typeof(TKey);
    public Type QueriesInterfaceType => typeof(TQueries);

    public required Type ViewModelType { get; init; }
    public Type? CreateViewModelType { get; init; }
    public Type? UpdateViewModelType { get; init; }
    public Type? FilteringViewModelType { get; init; }
}
