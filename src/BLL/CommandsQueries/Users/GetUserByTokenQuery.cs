using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Services;
using BLL.ViewModels.User;
using MediatR;

namespace BLL.CommandsQueries.Users;

public record GetUserByTokenQuery : IRequest<Result<UserVM?>>;

public class QueryHandler(IUserQueries userQueries, IMapper mapper, IUserProvider userProvider)
    : IRequestHandler<GetUserByTokenQuery, Result<UserVM?>>
{
    public async Task<Result<UserVM?>> Handle(GetUserByTokenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await userProvider.GetUserId();
            
            var user = await userQueries.GetByIdAsync(userId, cancellationToken);

            return Result<UserVM?>.Ok("Your profile retrieved", mapper.Map<UserVM>(user));
        }
        catch (Exception exception)
        {
            return Result<UserVM?>.InternalError(exception.Message);
        }
    }
}