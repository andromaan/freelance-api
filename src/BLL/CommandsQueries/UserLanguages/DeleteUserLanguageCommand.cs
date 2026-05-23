using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Languages;
using BLL.Common.Interfaces.Repositories.UserLanguages;
using BLL.Services;
using MediatR;

namespace BLL.CommandsQueries.UserLanguages;

public record DeleteUserLanguageCommand(int LanguageId) : IRequest<ServiceResponse<string?>>;

public class DeleteUserLanguageCommandHandler(
    IUserProvider userProvider,
    IUserLanguageRepository userLanguageRepository,
    IUserLanguageQueries userLanguageQueries,
    ILanguageQueries languageQueries) : IRequestHandler<DeleteUserLanguageCommand, ServiceResponse<string?>>
{
    public async Task<ServiceResponse<string?>> Handle(DeleteUserLanguageCommand request, CancellationToken cancellationToken)
    {
        var language = await languageQueries.GetByIdAsync(request.LanguageId, cancellationToken);
        if (language == null)
        {
            return ServiceResponse<string?>.NotFound("Language not found");
        }

        try
        {
            var existingUserLanguage = await userLanguageQueries.GetByIdAsync(request.LanguageId,
                await userProvider.GetUserId(cancellationToken), cancellationToken);

            if (existingUserLanguage == null)
            {
                return ServiceResponse<string?>.NotFound("User language not found");
            }

            await userLanguageRepository.DeleteAsync(existingUserLanguage.LanguageId, existingUserLanguage.UserId,
                cancellationToken);

            return ServiceResponse<string?>.Ok("User language deleted");
        }
        catch (Exception exception)
        {
            return ServiceResponse<string?>.InternalError(exception.Message);
        }
    }
}