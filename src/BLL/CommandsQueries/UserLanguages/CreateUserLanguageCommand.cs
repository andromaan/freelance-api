using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Languages;
using BLL.Common.Interfaces.Repositories.UserLanguages;
using BLL.Services;
using BLL.ViewModels.UserLanguage;
using Domain.Models.Users;
using MediatR;

namespace BLL.CommandsQueries.UserLanguages;

public record CreateUserLanguageCommand(CreateUserLanguageVM CreateModel) : IRequest<ServiceResponse<UserLanguageVM?>>;

public class CreateUserLanguageCommandHandler(
    IUserProvider userProvider,
    IMapper mapper,
    IUserLanguageRepository userLanguageRepository,
    ILanguageQueries languageQueries) : IRequestHandler<CreateUserLanguageCommand, ServiceResponse<UserLanguageVM?>>
{
    public async Task<ServiceResponse<UserLanguageVM?>> Handle(CreateUserLanguageCommand request, CancellationToken cancellationToken)
    {
        var language = await languageQueries.GetByIdAsync(request.CreateModel.LanguageId, cancellationToken);
        if (language == null)
        {
            return ServiceResponse<UserLanguageVM?>.NotFound("Language not found");
        }

        try
        {
            var userLanguage = mapper.Map<UserLanguage>(request.CreateModel);
            userLanguage.UserId = await userProvider.GetUserId(cancellationToken);

            var createdEntity = await userLanguageRepository.CreateAsync(userLanguage, cancellationToken);

            return ServiceResponse<UserLanguageVM?>.Ok("User language created", mapper.Map<UserLanguageVM>(createdEntity));
        }
        catch (Exception exception)
        {
            return ServiceResponse<UserLanguageVM?>.InternalError(exception.Message);
        }
    }
}