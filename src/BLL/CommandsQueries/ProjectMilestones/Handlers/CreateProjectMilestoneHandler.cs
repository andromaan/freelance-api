using System.Net;
using BLL.Common.Handlers;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.ProjectMilestones;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Services;
using BLL.ViewModels.ProjectMilestone;
using Domain.Models.Projects;

namespace BLL.CommandsQueries.ProjectMilestones.Handlers;

public class CreateProjectMilestoneHandler(
    IUserProvider userProvider,
    IProjectQueries projectQueries,
    IProjectMilestoneQueries milestoneQueries
) : ICreateHandler<ProjectMilestone, CreateProjectMilestoneVM, ProjectMilestoneVM>
{
    public async Task<ServiceResponse<ProjectMilestoneVM?>> HandleAsync(ProjectMilestone? entity,
        CreateProjectMilestoneVM createModel, CancellationToken cancellationToken)
    {
        var userRole = userProvider.GetUserRole();
        var userId = await userProvider.GetUserId();

        var existingProject = await projectQueries.GetByIdAsync(createModel.ProjectId, cancellationToken);

        if (existingProject is null)
        {
            return ServiceResponse<ProjectMilestoneVM?>.NotFound($"Project with Id {createModel.ProjectId} not found");
        }

        if (existingProject.CreatedBy != userId && userRole != Settings.Roles.AdminRole)
        {
            return ServiceResponse<ProjectMilestoneVM?>.Unauthorized("You are not authorized to create a milestone for this project");
        }

        var existingMilestones =
            await milestoneQueries.GetByProjectIdAsync(createModel.ProjectId, cancellationToken);

        var totalMilestoneAmount = existingMilestones.Sum(x => x.Amount) + createModel.Amount;
        if (totalMilestoneAmount > existingProject.Budget)
        {
            return ServiceResponse<ProjectMilestoneVM?>.GetResponse(
                $"The total amount ({totalMilestoneAmount}) of milestones exceeds " +
                $"the project's budged ({existingProject.Budget})",
                false, null, HttpStatusCode.BadRequest);
        }

        return ServiceResponse<ProjectMilestoneVM?>.Ok(); // Валідація пройшла успішно
    }
}