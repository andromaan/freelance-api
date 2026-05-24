using BLL.Common.Handlers;
using BLL.Common.Interfaces.Repositories.Bids;
using BLL.Common.Interfaces.Repositories.Categories;
using BLL.Common.Interfaces.Repositories.ContractMilestones;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Countries;
using BLL.Common.Interfaces.Repositories.DisputeResolutions;
using BLL.Common.Interfaces.Repositories.Disputes;
using BLL.Common.Interfaces.Repositories.Employers;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Common.Interfaces.Repositories.Languages;
using BLL.Common.Interfaces.Repositories.Messages;
using BLL.Common.Interfaces.Repositories.Notifications;
using BLL.Common.Interfaces.Repositories.Portfolios;
using BLL.Common.Interfaces.Repositories.ProjectMilestones;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Common.Interfaces.Repositories.Quotes;
using BLL.Common.Interfaces.Repositories.Reviews;
using BLL.Common.Interfaces.Repositories.Roles;
using BLL.Common.Interfaces.Repositories.Skills;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Extensions;
using BLL.ViewModels.Bid;
using BLL.ViewModels.Category;
using BLL.ViewModels.Contract;
using BLL.ViewModels.ContractMilestone;
using BLL.ViewModels.Country;
using BLL.ViewModels.Dispute;
using BLL.ViewModels.DisputeResolution;
using BLL.ViewModels.Employer;
using BLL.ViewModels.Freelancer;
using BLL.ViewModels.Language;
using BLL.ViewModels.Message;
using BLL.ViewModels.Notification;
using BLL.ViewModels.Portfolio;
using BLL.ViewModels.Project;
using BLL.ViewModels.ProjectMilestone;
using BLL.ViewModels.Quote;
using BLL.ViewModels.Reviews;
using BLL.ViewModels.Roles;
using BLL.ViewModels.Skill;
using BLL.ViewModels.User;
using Domain.Models.Auth;
using Domain.Models.Contracts;
using Domain.Models.Countries;
using Domain.Models.Disputes;
using Domain.Models.Employers;
using Domain.Models.Freelance;
using Domain.Models.Languages;
using Domain.Models.Messaging;
using Domain.Models.Notifications;
using Domain.Models.Projects;
using Domain.Models.Reviews;
using Domain.Models.Users;
using Microsoft.Extensions.DependencyInjection;

namespace BLL;

public static class ConfigureRegistrations
{
    internal static void AddRegistrations(this IServiceCollection services)
    {
        // Автоматична реєстрація всіх валідаторів, процесорів та нових хендлерів
        services.Scan(scan => scan
            .FromAssemblyOf<BLLClassForScanning>()
            // New unified handlers
            .AddClasses(classes => classes.AssignableTo(typeof(ICreateHandler<,,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IUpdateHandler<,,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IDeleteHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IGetAllFilteredHandler<,,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        // registrations for Roles
        services.AddQueriesHandlers<Role, int, RoleVM, IRoleQueries>();

        // registrations for Country
        services.RegisterCrudHandlers(
            new CrudRegistration<Country, int, ICountryQueries>
            {
                ViewModelType = typeof(CountryVM),
                CreateViewModelType = typeof(CreateCountryVM),
                UpdateViewModelType = typeof(UpdateCountryVM)
            });

        // registrations for Language
        services.RegisterCrudHandlers(
            new CrudRegistration<Language, int, ILanguageQueries>
            {
                ViewModelType = typeof(LanguageVM),
                CreateViewModelType = typeof(CreateLanguageVM),
                UpdateViewModelType = typeof(UpdateLanguageVM)
            });

        // registrations for Category
        services.RegisterCrudHandlers(
            new CrudRegistration<Category, int, ICategoryQueries>
            {
                ViewModelType = typeof(CategoryVM),
                CreateViewModelType = typeof(CreateCategoryVM),
                UpdateViewModelType = typeof(UpdateCategoryVM)
            });

        // registrations for Skill
        services.RegisterCrudHandlers(
            new CrudRegistration<Skill, int, ISkillQueries>
            {
                ViewModelType = typeof(SkillVM),
                CreateViewModelType = typeof(CreateSkillVM),
                UpdateViewModelType = typeof(UpdateSkillVM)
            });

        // registrations for Project
        services.RegisterCrudHandlers(
            new CrudRegistration<Project, Guid, IProjectQueries>
            {
                ViewModelType = typeof(ProjectVM),
                CreateViewModelType = typeof(CreateProjectVM),
                UpdateViewModelType = typeof(UpdateProjectVM),
                FilteringViewModelType = typeof(FilterProjectVM)
            }, specificUpdateVMs: [typeof(UpdateProjectCategoriesVM)]);

        // registrations for ProjectMilestone
        services.RegisterCrudHandlers(
            new CrudRegistration<ProjectMilestone, Guid, IProjectMilestoneQueries>
            {
                ViewModelType = typeof(ProjectMilestoneVM),
                CreateViewModelType = typeof(CreateProjectMilestoneVM),
                UpdateViewModelType = typeof(UpdateProjectMilestoneVM)
            });

        // registrations for ContractMilestone
        services.RegisterCrudHandlers(
            new CrudRegistration<ContractMilestone, Guid, IContractMilestoneQueries>
            {
                ViewModelType = typeof(ContractMilestoneVM),
                CreateViewModelType = typeof(CreateContractMilestoneVM),
                UpdateViewModelType = typeof(UpdateContractMilestoneVM)
            },
            specificUpdateVMs:
            [
                typeof(UpdContractMilestoneStatusEmployerVM),
                typeof(UpdContractMilestoneStatusFreelancerVM),
                typeof(UpdContractMilestoneStatusModeratorVM)
            ]);

        // registrations for Bids
        services.RegisterCrudHandlers(
            new CrudRegistration<Bid, Guid, IBidQueries>
            {
                ViewModelType = typeof(BidVM),
                CreateViewModelType = typeof(CreateBidVM),
                UpdateViewModelType = typeof(UpdateBidVM)
            });

        // registrations for Quotes
        services.RegisterCrudHandlers(
            new CrudRegistration<Quote, Guid, IQuoteQueries>
            {
                ViewModelType = typeof(QuoteVM),
                CreateViewModelType = typeof(CreateQuoteVM),
                UpdateViewModelType = typeof(UpdateQuoteVM)
            });

        // registrations for Messages
        services.RegisterCrudHandlers(
            new CrudRegistration<Message, Guid, IMessageQueries>
            {
                ViewModelType = typeof(MessageVM),
                CreateViewModelType = typeof(CreateMessageVM),
                UpdateViewModelType = typeof(UpdateMessageVM)
            },
            specificCreateVMs: [typeof(CreateMessageWithoutContractVM)]);

        // registrations for Contracts
        services.AddUpdateCommandHandler<Contract,
            Guid, ContractVM,
            UpdateContractVM,
            IContractQueries>(specificUpdateVMs: [typeof(UpdateContractStatusVM)]);

        // registrations for Freelancers
        services.AddUpdateByUserCommandHandler<Freelancer,
            Guid, FreelancerVM,
            UpdateFreelancerVM,
            IFreelancerQueries>(specificUpdateVMs:
            [typeof(UpdateFreelancerSkillsVM)]);

        // registrations for Employers
        services.AddUpdateByUserCommandHandler<Employer,
            Guid, EmployerVM,
            UpdateEmployerVM,
            IEmployerQueries>();

        // registrations for Reviews
        services.RegisterCrudHandlers(
            new CrudRegistration<Review, Guid, IReviewQueries>
            {
                ViewModelType = typeof(ReviewVM),
                CreateViewModelType = typeof(CreateReviewVM),
                UpdateViewModelType = typeof(UpdateReviewVM)
            });

        // registrations for Portfolios
        services.RegisterCrudHandlers(
            new CrudRegistration<Portfolio, Guid, IPortfolioQueries>
            {
                ViewModelType = typeof(PortfolioVM),
                CreateViewModelType = typeof(CreatePortfolioVM),
                UpdateViewModelType = typeof(UpdatePortfolioVM)
            });

        // registrations for Users
        services.RegisterCrudHandlers(
            new CrudRegistration<User, Guid, IUserQueries>
            {
                ViewModelType = typeof(UserVM),
                UpdateViewModelType = typeof(UpdateUserByAdminVM)
            });

        services.AddUpdateByUserCommandHandler<User, Guid, UserVM, UpdateUserVM, IUserQueries>
            ();

        // registrations for Disputes
        services.RegisterCrudHandlers(
            new CrudRegistration<Dispute, Guid, IDisputeQueries>
            {
                ViewModelType = typeof(DisputeVM),
                CreateViewModelType = typeof(CreateDisputeVM)
            }, specificUpdateVMs: [typeof(UpdateDisputeStatusForModeratorVM)]);

        // registrations for Dispute Resolutions
        services.RegisterCrudHandlers(
            new CrudRegistration<DisputeResolution, Guid, IDisputeResolutionQueries>
            {
                ViewModelType = typeof(DisputeResolutionVM),
                CreateViewModelType = typeof(CreateDisputeResolutionVM)
            });

        // registrations for Notifications
        services.AddUpdateByUserCommandHandler<
            Notification,
            Guid,
            NotificationVM,
            UpdateNotificationVM,
            INotificationQueries>();

        services.AddQueriesHandlers<Notification,
            Guid,
            NotificationVM,
            INotificationQueries>(filterViewModel: typeof(FilterNotificationVM));
    }
}