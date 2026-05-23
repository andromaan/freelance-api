using BLL.CommandsQueries.GenericCRUD.GetAll;
using BLL.CommandsQueries.GenericCRUD.GetById;
using BLL.Common.Interfaces.Repositories;
using BLL.Services;
using BLL.ViewModels;
using Domain.Common.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Extensions;

public static class QueryRegistrationExtensions
{
    public static IServiceCollection AddQueriesHandlers<TEntity, TKey, TViewModel, TQueries>(
        this IServiceCollection services, Type? filterViewModel = null)
        where TEntity : Entity<TKey>
        where TViewModel : class
        where TQueries : class, IQueries<TEntity, TKey>
    {
        services.AddTransient(
            typeof(IRequestHandler<GetAll.Query<TViewModel>, ServiceResponse<List<TViewModel>?>>),
            typeof(GetAll.QueryHandler<TEntity, TKey, TViewModel, TQueries>)
        );

        services.AddTransient(
            typeof(IRequestHandler<GetById.Query<TKey, TViewModel>, ServiceResponse<TViewModel?>>),
            typeof(GetById.QueryHandler<TEntity, TKey, TViewModel, TQueries>)
        );

        services.AddTransient(
            typeof(IRequestHandler<GetAllPaginated.Query<TViewModel>, ServiceResponse<PaginatedItemsVM<TViewModel>?>>),
            typeof(GetAllPaginated.QueryHandler<TEntity, TKey, TViewModel, TQueries>)
        );

        if (filterViewModel is not null)
        {
            var requestType = typeof(GetAllFilteredPaginated.Query<,>).MakeGenericType(filterViewModel, typeof(TViewModel));
            var handlerType = typeof(GetAllFilteredPaginated.QueryHandler<,,,,>)
                .MakeGenericType(typeof(TEntity), typeof(TKey), typeof(TViewModel), typeof(TQueries), filterViewModel);
            var responseType = typeof(ServiceResponse<>).MakeGenericType(
                typeof(PaginatedItemsVM<>).MakeGenericType(typeof(TViewModel)));
            var serviceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

            services.AddTransient(serviceType, handlerType);
        }

        return services;
    }
}
