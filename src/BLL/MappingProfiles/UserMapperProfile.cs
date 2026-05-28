using AutoMapper;
using BLL.ViewModels.Auth;
using BLL.ViewModels.User;
using Domain.Models.Users;

namespace BLL.MappingProfiles;

public class UserMapperProfile : Profile
{
    public UserMapperProfile()
    {
        CreateMap<SignUpVM, User>().ReverseMap();

        CreateMap<UpdateUserVM, User>().ReverseMap();
        
        CreateMap<User, UserVM>().ReverseMap();
        CreateMap<CreateUserByAdminVM, User>()
            .ForSourceMember(src => src.Password, opt => opt.DoNotValidate());
        CreateMap<UpdateUserByAdminVM, User>()
            .ForSourceMember(src => src.Password, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.Email, opt => opt.DoNotValidate());
    }
}