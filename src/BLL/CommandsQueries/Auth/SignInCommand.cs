using BLL.Common.Interfaces.Repositories.Users;
using BLL.Services;
using BLL.Services.JwtService;
using BLL.Services.PasswordHasher;
using BLL.ViewModels;
using BLL.ViewModels.Auth;
using MediatR;

namespace BLL.CommandsQueries.Auth;

public record SignInCommand(SignInVM Vm) : IRequest<ServiceResponse<JwtVM?>>;

public class SignInCommandHandler(
    IUserQueries userQueries,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtService) : IRequestHandler<SignInCommand, ServiceResponse<JwtVM?>>
{
    public async Task<ServiceResponse<JwtVM?>> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var vm = request.Vm;
        
        var user = await userQueries.GetByEmailAsync(vm.Email, cancellationToken);

        if (user == null)
        {
            return ServiceResponse<JwtVM?>.BadRequest($"Користувача з поштою {vm.Email} не знайдено");
        }

        var result = passwordHasher.Verify(vm.Password, user.PasswordHash);

        if (!result)
        {
            return ServiceResponse<JwtVM?>.BadRequest($"Пароль вказано невірно");
        }

        var tokens = await jwtService.GenerateTokensAsync(user, cancellationToken);

        return ServiceResponse<JwtVM?>.Ok("Успішний вхід", tokens);
    }
}