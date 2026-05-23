using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Employers;
using BLL.Services;
using BLL.ViewModels.Employer;
using MediatR;

namespace BLL.CommandsQueries.Employers;

public class GetEmployerByUserQuery : IRequest<ServiceResponse<EmployerVM?>>
{
}

public class QueryHandler(
    IEmployerQueries queriesEmployer,
    IUserProvider userProvider,
    IMapper mapper)
    : IRequestHandler<GetEmployerByUserQuery, ServiceResponse<EmployerVM?>>
{
    public async Task<ServiceResponse<EmployerVM?>> Handle(GetEmployerByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await userProvider.GetUserId();

            var employer = await queriesEmployer.GetByUserId(userId, cancellationToken);
            if (employer == null)
            {
                return ServiceResponse<EmployerVM?>.NotFound("Employer not found");
            }

            return ServiceResponse<EmployerVM?>.Ok("Employer retrieved",
                mapper.Map<EmployerVM>(employer));
        }
        catch (Exception exception)
        {
            return ServiceResponse<EmployerVM?>.InternalError(exception.Message);
        }
    }
}