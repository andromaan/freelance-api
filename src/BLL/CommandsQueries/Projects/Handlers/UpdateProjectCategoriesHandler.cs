using BLL.Common.Handlers;
using BLL.Common.Interfaces.Repositories.Categories;
using BLL.Services;
using BLL.ViewModels.Project;
using Domain.Models.Projects;

namespace BLL.CommandsQueries.Projects.Handlers;

public class UpdateProjectCategoriesHandler(
    ICategoryQueries categoryQueries
    ) : IUpdateHandler<Project, UpdateProjectCategoriesVM, ProjectVM>
{
    public async Task<ServiceResponse<ProjectVM?>> HandleAsync(Project existingEntity, UpdateProjectCategoriesVM updateModel,
        CancellationToken cancellationToken)
    {
        var categoriesIds = updateModel.CategoryIds.Distinct();

        existingEntity.Categories.Clear();

        foreach (var categId in categoriesIds)
        {
            var existingCategory = await categoryQueries.GetByIdAsync(categId, cancellationToken);
            if (existingCategory == null)
            {
                return ServiceResponse<ProjectVM?>.NotFound($"Category with id {categId} not found");
            }

            existingEntity.Categories.Add(existingCategory);
        }
        
        return ServiceResponse<ProjectVM?>.Ok();
    }
}