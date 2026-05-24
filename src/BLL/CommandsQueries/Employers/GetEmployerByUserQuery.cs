using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Employers;
using BLL.Services;
using BLL.ViewModels.Employer;
using MediatR;

namespace BLL.CommandsQueries.Employers;

public class GetEmployerByUserQuery : IRequest<Result<EmployerVM?>>
{
}

public class QueryHandler(
    IEmployerQueries queriesEmployer,
    IUserProvider userProvider,
    IMapper mapper)
    : IRequestHandler<GetEmployerByUserQuery, Result<EmployerVM?>>
{
    public async Task<Result<EmployerVM?>> Handle(GetEmployerByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await userProvider.GetUserId();

            var employer = await queriesEmployer.GetByUserId(userId, cancellationToken);
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