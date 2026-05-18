using AutoMapper;
using BLL.ViewModels.ProjectMilestone;
using Domain.Models.Projects;

namespace BLL.MappingProfiles;

public class ProjectMilestoneMapperProfile : Profile
{
    public ProjectMilestoneMapperProfile()
    {
        CreateMap<ProjectMilestone, ProjectMilestoneVM>().ReverseMap();
        CreateMap<ProjectMilestone, CreateProjectMilestoneVM>().ReverseMap();
        CreateMap<ProjectMilestone, UpdateProjectMilestoneVM>().ReverseMap();
    }
}