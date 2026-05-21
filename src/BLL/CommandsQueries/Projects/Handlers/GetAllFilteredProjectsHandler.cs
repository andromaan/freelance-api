using BLL.Common.Handlers;
using BLL.Common.Interfaces.Repositories.Categories;
using BLL.Services;
using BLL.ViewModels.Project;
using Domain.Models.Projects;

namespace BLL.CommandsQueries.Projects.Handlers;

public class GetAllFilteredProjectsHandler(ICategoryQueries categoryQueries)
    : IGetAllFilteredHandler<Project, FilterProjectVM>
{
    public async Task<(ServiceResponse response, List<Project>? filteredEntities)> HandleAsync(
        List<Project> entities, FilterProjectVM filter,
        CancellationToken cancellationToken)
    {
        List<Project> filteredEntities = entities.Where(e => e.Status == ProjectStatus.Open).ToList();

        if (filter.Title != null)
        {
            filteredEntities = filteredEntities
                .Where(e => e.Title.IndexOf(filter.Title, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        if (filter.Description != null)
        {
            filteredEntities = filteredEntities
                .Where(e => e.Description != null && 
                            e.Description.IndexOf(filter.Description, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        if (filter.BudgetMin != null)
        {
            filteredEntities = filteredEntities.Where(e => e.Budget >= filter.BudgetMin).ToList();
        }

        if (filter.BudgetMax != null)
        {
            filteredEntities = filteredEntities.Where(e => e.Budget <= filter.BudgetMax).ToList();
        }

        if (filter.DeadlineMax != null)
        {
            filteredEntities = filteredEntities.Where(e => e.Deadline <= filter.DeadlineMax).ToList();
        }

        if (filter.CategoryIds != null && filter.CategoryIds.Count > 0)
        {
            foreach (var filterCategoryId in filter.CategoryIds)
            {
                var category = await categoryQueries.GetByIdAsync(filterCategoryId, cancellationToken);
                if (category == null)
                {
                    return (ServiceResponse.NotFound($"Category with id {filterCategoryId} not found"), null);
                }

                filteredEntities = filteredEntities.Where(e => e.Categories.Any(c => c.Id == filterCategoryId)).ToList();
            }
        }

        return (ServiceResponse.Ok(), filteredEntities);
    }
}