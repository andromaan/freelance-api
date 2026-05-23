using AutoMapper;
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
using Microsoft.Extensions.Options;
using Stripe;

namespace BLL.CommandsQueries.Auth;

public record SignUpCommand(SignUpVM Vm) : IRequest<ServiceResponse<JwtVM?>>;

public class SignUpCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUserQueries userQueries,
    IMapper mapper,
    IFreelancerRepository freelancerRepository,
    IEmployerRepository employerRepository,
    IUserWalletRepository userWalletRepository,
    IRoleQueries roleQueries,
    IOptions<StripeModel> stripeModel,
    CustomerService customerService) : IRequestHandler<SignUpCommand, ServiceResponse<JwtVM?>>
{
    private readonly StripeModel _stripeModel = stripeModel.Value;

    public async Task<ServiceResponse<JwtVM?>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        var vm = request.Vm;
        if (!await userQueries.IsUniqueEmailAsync(vm.Email, cancellationToken))
        {
            return ServiceResponse<JwtVM?>.BadRequest($"{vm.Email} already exists");
        }

        if (vm is not { UserRole: Settings.Roles.EmployerRole or Settings.Roles.FreelancerRole })
        {
            return ServiceResponse<JwtVM?>.BadRequest(
                $"Invalid user role, must be '{Settings.Roles.EmployerRole}' or '{Settings.Roles.FreelancerRole}'");
        }

        var isDbHasUsers = (await userQueries.GetAllAsync(cancellationToken)).Count() != 0;
        var userRole = isDbHasUsers ? vm.UserRole : Settings.Roles.AdminRole;

        var roleEntity = await roleQueries.GetByNameAsync(userRole, cancellationToken);
        if (roleEntity is null)
        {
            return ServiceResponse<JwtVM?>.InternalError("User role not found in database");
        }

        var customer = await CreateStripeCustomerAsync(vm.Email, vm.DisplayName, cancellationToken);

        var user = mapper.Map<User>(vm);
        user.Id = Guid.NewGuid();
        user.PasswordHash = passwordHasher.HashPassword(vm.Password);
        user.CreatedBy = user.Id;
        user.RoleId = roleEntity.Id;
        user.Role = roleEntity;
        user.StripeCustomerId = customer.Id;

        try
        {
            await userRepository.CreateAsync(user, cancellationToken);

            if (user.Role.Name == Settings.Roles.FreelancerRole)
            {
                var freelancer = new Freelancer
                {
                    Id = Guid.NewGuid(),
                    CreatedBy = user.Id,
                };

                await freelancerRepository.CreateAsync(freelancer, user.Id, cancellationToken);
            }

            if (user.Role.Name == Settings.Roles.EmployerRole)
            {
                var employer = new Employer
                {
                    Id = Guid.NewGuid(),
                    CreatedBy = user.Id,
                };

                await employerRepository.CreateAsync(employer, user.Id, cancellationToken);
            }

            if (user.Role.Name != Settings.Roles.AdminRole)
            {
                var userWallet = new UserWallet
                {
                    Id = Guid.NewGuid(),
                    Currency = "UAH",
                    CreatedBy = user.Id,
                    Balance = 0m
                };

                await userWalletRepository.CreateAsync(userWallet, cancellationToken);
            }
        }
        catch (Exception e)
        {
            return ServiceResponse<JwtVM?>.InternalError(e.Message);
        }

        var tokens = await jwtTokenService.GenerateTokensAsync(user, cancellationToken);

        return ServiceResponse<JwtVM?>.Ok($"User {vm.Email} successfully created", tokens);
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