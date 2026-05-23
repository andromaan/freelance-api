using BLL.Common.Handlers;
using BLL.Common.Interfaces.Repositories.Countries;
using BLL.Services;
using BLL.ViewModels.User;
using Domain.Models.Users;

namespace BLL.CommandsQueries.Users.Handlers;

public class UpdateUserHandler(ICountryQueries countryQueries) : IUpdateHandler<User, UpdateUserVM, UserVM>
{
    public async Task<ServiceResponse<UserVM?>> HandleAsync(User existingEntity, UpdateUserVM updateModel,
        CancellationToken cancellationToken)
    {
        if (await countryQueries.GetByIdAsync(updateModel.CountryId, cancellationToken) == null)
        {
            return ServiceResponse<UserVM?>.NotFound($"Country with id {updateModel.CountryId} not found");
        }
        
        return ServiceResponse<UserVM?>.Ok();
    }
}