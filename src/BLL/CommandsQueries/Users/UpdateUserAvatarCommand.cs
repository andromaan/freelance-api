using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Services;
using BLL.Services.ImageService;
using BLL.ViewModels.User;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BLL.CommandsQueries.Users;

public record UpdateUserAvatarCommand(IFormFile AvatarImg) : IRequest<ServiceResponse<UserVM?>>;

public class UpdateUserAvatarCommandHandler(
    IUserProvider userProvider,
    IImageService imageService,
    IUserQueries userQueries,
    IUserRepository userRepository,
    IMapper mapper)
    : IRequestHandler<UpdateUserAvatarCommand, ServiceResponse<UserVM?>>
{
    public async Task<ServiceResponse<UserVM?>> Handle(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
    {
        if (request.AvatarImg.Length == 0)
        {
            return ServiceResponse<UserVM?>.BadRequest("No image uploaded");
        }
        
        var userId = await userProvider.GetUserId();
        var existingEntity = await userQueries.GetByIdAsync(userId, cancellationToken);
        
        var imageName = existingEntity!.AvatarImg?.Split('/').LastOrDefault();

        var newImageName =
            await imageService.SaveImageFromFileAsync(Settings.ImagesPathSettings.UserAvatarImagesPath, request.AvatarImg, imageName);
        
        if (newImageName == null)
        {
            return ServiceResponse<UserVM?>.BadRequest("No image uploaded");
        }

        existingEntity.AvatarImg = $"{Settings.ImagesPathSettings.UserAvatarImagesPathForUrl}/{newImageName}";
        
        try
        {
            await userRepository.UpdateAsync(existingEntity, cancellationToken);
            return ServiceResponse<UserVM?>.Ok(
                $"User avatar updated successfully",
                mapper.Map<UserVM>(existingEntity));
        }
        catch (Exception exception)
        {
            return ServiceResponse<UserVM?>.InternalError(exception.Message);
        }
    }
}