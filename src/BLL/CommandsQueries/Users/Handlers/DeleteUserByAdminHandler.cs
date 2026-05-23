using BLL.Common.Handlers;
using BLL.Services;
using BLL.Services.ImageService;
using Domain.Models.Users;

namespace BLL.CommandsQueries.Users.Handlers;

public class DeleteUserByAdminHandler(IImageService imageService) : IDeleteHandler<User, string>
{
    public Task<ServiceResponse<string?>> HandleAsync(User entity, CancellationToken cancellationToken)
    {
        if (entity.AvatarImg != null)
        {
            var isAvatarDeleted = imageService.DeleteImage(Settings.ImagesPathSettings.UserAvatarImagesPath, entity.AvatarImg);
            if (!isAvatarDeleted)
            {
                return Task.FromResult(
                    ServiceResponse<string?>.InternalError("Failed to delete user avatar image."));
            }
        }

        return Task.FromResult(ServiceResponse<string?>.Ok());
    }
}