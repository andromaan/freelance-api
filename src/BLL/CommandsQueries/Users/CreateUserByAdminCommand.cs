using AutoMapper;
using BLL.Common.Interfaces.Repositories.Employers;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Common.Interfaces.Repositories.Roles;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Common.Interfaces.Repositories.UserWallets;
using BLL.Services;
using BLL.Services.PasswordHasher;
using BLL.ViewModels.User;
using Domain.Models.Employers;
using Domain.Models.Freelance;
using Domain.Models.Payments;
using Domain.Models.Users;
using MediatR;

namespace BLL.CommandsQueries.Users;

public record CreateUserByAdminCommand(CreateUserByAdminVM CreateModel) : IRequest<Result<UserVM?>>;

public class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUserQueries userQueries,
    IFreelancerRepository freelancerRepository,
    IEmployerRepository employerRepository,
    IUserWalletRepository userWalletRepository,
    IMapper mapper,
    IRoleQueries roleQueries) : IRequestHandler<CreateUserByAdminCommand, Result<UserVM?>>
{
    public async Task<Result<UserVM?>> Handle(CreateUserByAdminCommand request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<User>(request.CreateModel);
        
        var createModel = request.CreateModel;
        
        var role = await roleQueries.GetByIdAsync(createModel.RoleId, cancellationToken);
        
        if (role is null)
        {
            return Result<UserVM?>.BadRequest($"Role with Id {createModel.RoleId} not found.");
        }

        var userWithEmail = await userQueries.GetByEmailAsync(createModel.Email, cancellationToken);
        if (userWithEmail != null)
        {
            return Result<UserVM?>.BadRequest($"A user with the email {createModel.Email} already exists.");
        }

        entity.PasswordHash = passwordHasher.HashPassword(createModel.Password);
        
        try
        {
            var createdEntity = await userRepository.CreateAsync(entity, cancellationToken);
            
            await ConfigureUserBaseOfRole(entity, cancellationToken);
            
            return Result<UserVM?>.Ok($"User created",
                mapper.Map<UserVM>(createdEntity));
        }
        catch (Exception exception)
        {
            return Result<UserVM?>.InternalError(exception.Message);
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
                CreatedBy = createdUser.Id,
                Balance = 0m
            };

            await userWalletRepository.CreateAsync(userWallet, cancellationToken);
        }
    }
}