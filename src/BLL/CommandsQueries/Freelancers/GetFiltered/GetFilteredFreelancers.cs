using AutoMapper;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.Country;
using BLL.ViewModels.Freelancer;
using BLL.ViewModels.Skill;
using BLL.ViewModels.UserLanguage;
using MediatR;

namespace BLL.CommandsQueries.Freelancers.GetFiltered;

public class GetFilteredFreelancers
{
    public record Query(PagedVM PagedVm, FilterFreelancerVM Filter) : IRequest<Result<PaginatedItemsVM<SearchFreelancerVM>>>;

    public class QueryHandler(IFreelancerQueries freelancerQueries, IMapper mapper) : IRequestHandler<Query, Result<PaginatedItemsVM<SearchFreelancerVM>>>
    {
        public async Task<Result<PaginatedItemsVM<SearchFreelancerVM>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var filter = request.Filter;
            var page = request.PagedVm.Page;
            var pageSize = request.PagedVm.PageSize;

            var (totalCount, items) = await freelancerQueries.SearchFreelancersAsync(filter, page, pageSize, cancellationToken);

            var mappedItems = items.Select(item => new SearchFreelancerVM
            {
                Id = item.Freelancer.Id,
                UserId = item.User.Id,
                DisplayName = item.User.DisplayName,
                Email = item.User.Email,
                AvatarImg = item.User.AvatarImg,
                Bio = item.Freelancer.Bio,
                Location = item.Freelancer.Location,
                Skills = mapper.Map<List<SkillVM>>(item.Freelancer.Skills),
                Languages = mapper.Map<List<UserLanguageVM>>(item.User.Languages),
                Country = mapper.Map<CountryVM>(item.User.Country),
                Rating = item.Rating,
                ReviewsCount = item.ReviewsCount
            }).ToList();

            var pagedResponse = new PaginatedItemsVM<SearchFreelancerVM>
            {
                Items = mappedItems,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                PageCount = (int)Math.Ceiling((double)totalCount / pageSize),
            };

            return Result<PaginatedItemsVM<SearchFreelancerVM>>.Ok("Freelancers retrieved", pagedResponse);
        }
    }
}
