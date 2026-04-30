using Api.Controllers.Models.Request;
using Application.DTO;
using AutoMapper;

namespace Api.Mappings;

/// <summary>
/// Маппер для user
/// </summary>
public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<CreateUserRequest, UserDto>()
            .ForMember(x => x.CurrentUserId, y => y.Ignore())
            .ForMember(x => x.Email, y => y.MapFrom(z => z.Email))
            .ForMember(x => x.FullName, y => y.MapFrom(z => z.FullName))
            .ForMember(x => x.Password, y => y.MapFrom(z => z.Password))
            .ForMember(x => x.Role, y => y.MapFrom(z => z.Role));
    }
}