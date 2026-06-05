using AutoMapper;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.User;
using MediatR;

namespace BLL.CommandsQueries.Users.GetFiltered;

public class GetFilteredUsers
{
    public record Query(PagedVM PagedVm, FilterUserVM Filter) : IRequest<Result<PaginatedItemsVM<UserVM>>>;

    public class QueryHandler(IUserQueries userQueries, IMapper mapper) : IRequestHandler<Query, Result<PaginatedItemsVM<UserVM>>>
    {
        public async Task<Result<PaginatedItemsVM<UserVM>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var filter = request.Filter;
            var page = request.PagedVm.Page;
            var pageSize = request.PagedVm.PageSize;

            var (totalCount, items) = await userQueries.SearchUsersAsync(filter, page, pageSize, cancellationToken);

            var mappedItems = mapper.Map<List<UserVM>>(items);

            var pagedResponse = new PaginatedItemsVM<UserVM>
            {
                Items = mappedItems,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                PageCount = (int)Math.Ceiling((double)totalCount / pageSize),
            };

            return Result<PaginatedItemsVM<UserVM>>.Ok("Users retrieved", pagedResponse);
        }
    }
}
