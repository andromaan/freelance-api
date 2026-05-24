using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Languages;
using BLL.Common.Interfaces.Repositories.UserLanguages;
using BLL.Services;
using MediatR;

namespace BLL.CommandsQueries.UserLanguages;

public record DeleteUserLanguageCommand(int LanguageId) : IRequest<Result<string?>>;

public class DeleteUserLanguageCommandHandler(
    IUserProvider userProvider,
    IUserLanguageRepository userLanguageRepository,
    IUserLanguageQueries userLanguageQueries,
    ILanguageQueries languageQueries) : IRequestHandler<DeleteUserLanguageCommand, Result<string?>>
{
    public async Task<Result<string?>> Handle(DeleteUserLanguageCommand request, CancellationToken cancellationToken)
    {
        var language = await languageQueries.GetByIdAsync(request.LanguageId, cancellationToken);
        if (language == null)
        {
            return Result<string?>.NotFound("Language not found");
        }

        try
        {
            var existingUserLanguage = await userLanguageQueries.GetByIdAsync(request.LanguageId,
                await userProvider.GetUserId(cancellationToken), cancellationToken);

            if (existingUserLanguage == null)
            {
                return Result<string?>.NotFound("User language not found");
            }

            await userLanguageRepository.DeleteAsync(existingUserLanguage.LanguageId, existingUserLanguage.UserId,
                cancellationToken);

            return Result<string?>.Ok("User language deleted");
        }
        catch (Exception exception)
        {
            return Result<string?>.InternalError(exception.Message);
        }
    }
}