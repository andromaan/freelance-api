using AutoMapper;
using BLL.Common.Interfaces.Repositories.Employers;
using BLL.Services;
using BLL.ViewModels.Employer;
using MediatR;

namespace BLL.CommandsQueries.Employers;

public record GetEmployerByIdQuery(Guid EmployerId) : IRequest<Result<EmployerVM?>>;

public class GetEmployerByIdQueryQueryHandler(
    IEmployerQueries queriesEmployer,
    IMapper mapper)
    : IRequestHandler<GetEmployerByIdQuery, Result<EmployerVM?>>
{
    public async Task<Result<EmployerVM?>> Handle(GetEmployerByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var employer = await queriesEmployer.GetByIdAsync(request.EmployerId, cancellationToken);
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