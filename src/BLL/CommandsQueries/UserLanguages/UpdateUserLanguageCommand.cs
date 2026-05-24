using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Languages;
using BLL.Common.Interfaces.Repositories.UserLanguages;
using BLL.Services;
using BLL.ViewModels.UserLanguage;
using Domain.Models.Users;
using MediatR;

namespace BLL.CommandsQueries.UserLanguages;

public record UpdateUserLanguageCommand(UpdateUserLanguageVM UpdateModel) : IRequest<Result<UserLanguageVM?>>;

public class UpdateUserLanguageCommandHandler(
    IUserProvider userProvider,
    IMapper mapper,
    IUserLanguageRepository userLanguageRepository,
    ILanguageQueries languageQueries) : IRequestHandler<UpdateUserLanguageCommand, Result<UserLanguageVM?>>
{
    public async Task<Result<UserLanguageVM?>> Handle(UpdateUserLanguageCommand request, CancellationToken cancellationToken)
    {
        var language = await languageQueries.GetByIdAsync(request.UpdateModel.LanguageId, cancellationToken);
        if (language == null)
        {
            return Result<UserLanguageVM?>.NotFound("Language not found");
        }

        try
        {
            var userLanguage = mapper.Map<UserLanguage>(request.UpdateModel);
            userLanguage.UserId = await userProvider.GetUserId(cancellationToken);

            var updatedEntity = await userLanguageRepository.UpdateAsync(userLanguage, cancellationToken);

            return Result<UserLanguageVM?>.Ok("User language updated", mapper.Map<UserLanguageVM>(updatedEntity));
        }
        catch (Exception exception)
        {
            return Result<UserLanguageVM?>.InternalError(exception.Message);
        }
    }
}