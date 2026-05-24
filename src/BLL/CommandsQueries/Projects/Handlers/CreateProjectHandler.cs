using BLL.Common.Handlers;
using BLL.Common.Interfaces.Repositories.Categories;
using BLL.Services;
using BLL.ViewModels.Project;
using Domain.Models.Projects;

namespace BLL.CommandsQueries.Projects.Handlers;

public class CreateProjectHandler(ICategoryQueries categoryQueries) : ICreateHandler<Project, CreateProjectVM, ProjectVM>
{
    public async Task<Result<ProjectVM?>> HandleAsync(Project entity, CreateProjectVM createModel, CancellationToken cancellationToken)
    {
        var categoriesIds = createModel.CategoryIds.Distinct();
        
        foreach (var categId in categoriesIds)
        {
            var existingCategory = await categoryQueries.GetByIdAsync(categId, cancellationToken);
            if (existingCategory == null)
            {
                return Result<ProjectVM?>.NotFound($"Category with id {categId} not found");
            }

            entity.Categories.Add(existingCategory);
        }
        
        return Result<ProjectVM?>.Ok();
    }
}