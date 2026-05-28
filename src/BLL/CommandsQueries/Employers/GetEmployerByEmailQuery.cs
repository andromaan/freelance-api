using AutoMapper;
using BLL.Common.Interfaces.Repositories.Employers;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Services;
using BLL.ViewModels.Employer;
using MediatR;

namespace BLL.CommandsQueries.Employers;

public record GetEmployerByEmailQuery(string Email) : IRequest<Result<EmployerVM?>>;

public class GetEmployerByEmailQueryHandler(
    IEmployerQueries queriesEmployer,
    IMapper mapper,
    IUserQueries userQueries)
    : IRequestHandler<GetEmployerByEmailQuery, Result<EmployerVM?>>
{
    public async Task<Result<EmployerVM?>> Handle(GetEmployerByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userQueries.GetByEmailAsync(request.Email, cancellationToken);
            
            if (user == null)
            {
                return Result<EmployerVM?>.NotFound("User not found with the provided email");
            }

            var employer = await queriesEmployer.GetByUserIdAsync(user.Id, cancellationToken);
            if (employer == null)
            {
                return Result<EmployerVM?>.NotFound("Employer not found");
            }

            return Result<EmployerVM?>.Ok("Employer retrieved",
                mapper.Map<EmployerVM>(employer));
        }
        catch (Exception exception)
        {
            return Result<EmployerVM?>.InternalError(exception.Message);
        }
    }
}