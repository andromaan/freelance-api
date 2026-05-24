using BLL.Common.Interfaces.Repositories.Employers;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Common.Interfaces.Repositories.Roles;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Common.Interfaces.Repositories.UserWallets;
using BLL.Models;
using BLL.Services;
using BLL.Services.JwtService;
using BLL.Services.PasswordHasher;
using BLL.ViewModels;
using BLL.ViewModels.Auth;
using Domain.Models.Employers;
using Domain.Models.Freelance;
using Domain.Models.Payments;
using Domain.Models.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Stripe;

namespace BLL.CommandsQueries.Auth;

public record GoogleExternalLoginCommand : IRequest<Result<JwtVM?>>
{
    public required ExternalLoginVM Model { get; init; }
}

public class GoogleExternalLoginCommandHandler(
    IUserRepository userRepository,
    IUserQueries userQueries,
    IJwtTokenService jwtTokenService,
    IPasswordHasher hashPasswordService,
    IFreelancerRepository freelancerRepository,
    IEmployerRepository employerRepository,
    IUserWalletRepository userWalletRepository,
    IRoleQueries roleQueries,
    IOptions<StripeModel> stripeModel,
    CustomerService customerService)
    : IRequestHandler<GoogleExternalLoginCommand, Result<JwtVM?>>
{
    private readonly StripeModel _stripeModel = stripeModel.Value;

    public async Task<Result<JwtVM?>> Handle(GoogleExternalLoginCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Model.Token))
                return Result<JwtVM?>.BadRequest("Google token not sent");

            var payload = await jwtTokenService.VerifyGoogleToken(request.Model);

            var info = new UserLoginInfo(request.Model.Provider, payload.Subject, request.Model.Provider);

            var user = await userQueries.FindByLoginAsync(info.LoginProvider, info.ProviderKey, cancellationToken);
            if (user == null)
            {
                user = await userQueries.GetByEmailAsync(payload.Email, cancellationToken);
                if (user == null)
                {
                    var userId = Guid.NewGuid();
                    var randomPassword = GenerateRandomPassword();

                    var fullName = SplitFullName(payload.Name);

                    if (request.Model.UserRole is null)
                    {
                        return Result<JwtVM?>.BadRequest(
                            "User role must be provided for new users",
                            data: new JwtVM { AccessToken = "role_required", RefreshToken = "role_required" });
                    }

                    if (request.Model.UserRole is not (Settings.Roles.EmployerRole or Settings.Roles.FreelancerRole))
                    {
                        return Result<JwtVM?>.BadRequest(
                            $"Invalid user role, must be '{Settings.Roles.EmployerRole}' or '{Settings.Roles.FreelancerRole}'");
                    }

                    var isDbHasUsers = (await userQueries.GetAllAsync(cancellationToken)).Count() != 0;
                    var userRole = isDbHasUsers ? request.Model.UserRole : Settings.Roles.AdminRole;

                    var roleEntity = await roleQueries.GetByNameAsync(userRole, cancellationToken);
                    if (roleEntity is null)
                    {
                        return Result<JwtVM?>.InternalError("User role not found in database");
                    }

                    var customer = await CreateStripeCustomerAsync(payload.Email, fullName.name ?? payload.Name,
                        cancellationToken);

                    var userModel = new User
                    {
                        Id = userId,
                        Email = payload.Email,
                        DisplayName = fullName.name,
                        RoleId = roleEntity.Id,
                        Role = roleEntity,
                        PasswordHash = hashPasswordService.HashPassword(randomPassword),
                        StripeCustomerId = customer.Id,
                        CreatedBy = userId
                    };


                    var createdUser = await userRepository.CreateAsync(userModel, cancellationToken);

                    if (createdUser is null)
                    {
                        return Result<JwtVM?>.InternalError("Failed to add user");
                    }

                    await ConfigureUserBaseOfRole(createdUser, cancellationToken);

                    user = createdUser;
                }

                var loginResult = await userRepository.AddLoginAsync(user, info, cancellationToken);
                user = loginResult.Succeeded ? user : null;
            }

            if (user is null)
                return Result<JwtVM?>.BadRequest("Failed to add Google login");

            var tokens = await jwtTokenService.GenerateTokensAsync(user, cancellationToken);
            return Result<JwtVM?>.Ok("Users tokens", tokens);
        }
        catch (Exception ex)
        {
            return Result<JwtVM?>.InternalError(ex.Message);
        }
    }

    private async Task ConfigureUserBaseOfRole(User createdUser, CancellationToken cancellationToken)
    {
        if (createdUser.Role!.Name == Settings.Roles.FreelancerRole)
        {
            var freelancer = new Freelancer
            {
                Id = Guid.NewGuid(),
                CreatedBy = createdUser.Id,
            };

            await freelancerRepository.CreateAsync(freelancer, createdUser.Id, cancellationToken);
        }

        if (createdUser.Role.Name == Settings.Roles.EmployerRole)
        {
            var employer = new Employer
            {
                Id = Guid.NewGuid(),
                CreatedBy = createdUser.Id,
            };

            await employerRepository.CreateAsync(employer, createdUser.Id, cancellationToken);
        }

        if (createdUser.Role.Name != Settings.Roles.AdminRole)
        {
            var userWallet = new UserWallet
            {
                Id = Guid.NewGuid(),
                Currency = "UAH",
                CreatedBy = createdUser.Id,
                Balance = 0m
            };

            await userWalletRepository.CreateAsync(userWallet, cancellationToken);
        }
    }

    private (string? name, string? patronymic) SplitFullName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return (null, null);

        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var name = parts.ElementAtOrDefault(0);
        var patronymic = parts.ElementAtOrDefault(1);

        return (name, patronymic);
    }

    private string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private async Task<Customer> CreateStripeCustomerAsync(string email, string? displayName,
        CancellationToken cancellationToken)
    {
        StripeConfiguration.ApiKey = _stripeModel.SecretKey;

        var customerOptions = new CustomerCreateOptions
        {
            Email = email,
            Name = displayName
        };

        var customer = await customerService.CreateAsync(customerOptions, cancellationToken: cancellationToken);

        return customer;
    }
}