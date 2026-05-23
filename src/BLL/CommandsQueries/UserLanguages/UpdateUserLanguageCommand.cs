using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Languages;
using BLL.Common.Interfaces.Repositories.UserLanguages;
using BLL.Services;
using BLL.ViewModels.UserLanguage;
using Domain.Models.Users;
using MediatR;

namespace BLL.CommandsQueries.UserLanguages;

public record UpdateUserLanguageCommand(UpdateUserLanguageVM UpdateModel) : IRequest<ServiceResponse<UserLanguageVM?>>;

public class UpdateUserLanguageCommandHandler(
    IUserProvider userProvider,
    IMapper mapper,
    IUserLanguageRepository userLanguageRepository,
    ILanguageQueries languageQueries) : IRequestHandler<UpdateUserLanguageCommand, ServiceResponse<UserLanguageVM?>>
{
    public async Task<ServiceResponse<UserLanguageVM?>> Handle(UpdateUserLanguageCommand request, CancellationToken cancellationToken)
    {
        var language = await languageQueries.GetByIdAsync(request.UpdateModel.LanguageId, cancellationToken);
        if (language == null)
        {
            return ServiceResponse<UserLanguageVM?>.NotFound("Language not found");
        }

        try
        {
            var userLanguage = mapper.Map<UserLanguage>(request.UpdateModel);
            userLanguage.UserId = await userProvider.GetUserId(cancellationToken);

            var updatedEntity = await userLanguageRepository.UpdateAsync(userLanguage, cancellationToken);

            return ServiceResponse<UserLanguageVM?>.Ok("User language updated", mapper.Map<UserLanguageVM>(updatedEntity));
        }
        catch (Exception exception)
        {
            return ServiceResponse<UserLanguageVM?>.InternalError(exception.Message);
        }
    }
}