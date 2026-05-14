using Api.Controllers.Models.Request;
using Api.Controllers.Models.Response;
using Application.DTO.Auth;
using Application.DTO.User;
using AutoMapper;
using Domain.User;

namespace Api.Mappings;

/// <summary>
/// Маппер пользователей.
/// </summary>
public sealed class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<CreateUserRequest, UserDto>()
            .ForMember(x => x.Email, y => y.MapFrom(z => z.Email))
            .ForMember(x => x.FullName, y => y.MapFrom(z => z.FullName))
            .ForMember(x => x.Password, y => y.MapFrom(z => z.Password))
            .ForMember(x => x.Role, y => y.MapFrom(z => z.Role));

        CreateMap<UpdateUserRequest, UpdateUserDto>()
            .ForMember(x => x.Id, y => y.Ignore())
            .ForMember(x => x.Email, y => y.MapFrom(z => z.Email))
            .ForMember(x => x.FullName, y => y.MapFrom(z => z.FullName))
            .ForMember(x => x.Password, y => y.MapFrom(z => z.Password))
            .ForMember(x => x.Role, y => y.MapFrom(z => z.Role))
            .ForMember(x => x.IsActive, y => y.MapFrom(z => z.IsActive))
            .ForMember(x => x.MustChangePassword, y => y.MapFrom(z => z.MustChangePassword));

        CreateMap<UserModel, UserResponse>()
            .ForMember(x => x.Id, y => y.MapFrom(z => z.Id))
            .ForMember(x => x.Email, y => y.MapFrom(z => z.Email.Value))
            .ForMember(x => x.FullName, y => y.MapFrom(z => z.FullName.Value))
            .ForMember(x => x.Role, y => y.MapFrom(z => z.Role))
            .ForMember(x => x.IsActive, y => y.MapFrom(z => z.IsActive))
            .ForMember(x => x.MustChangePassword, y => y.MapFrom(z => z.MustChangePassword))
            .ForMember(x => x.CreatedBy, y => y.MapFrom(z => z.CreatedBy))
            .ForMember(x => x.CreatedAt, y => y.MapFrom(z => z.CreatedAt))
            .ForMember(x => x.UpdatedAt, y => y.MapFrom(z => z.UpdatedAt));
        
        CreateMap<ChangePasswordRequest, ChangePasswordDto>()
            .ForMember(x => x.CurrentPassword, y => y.MapFrom(z => z.CurrentPassword))
            .ForMember(x => x.NewPassword, y => y.MapFrom(z => z.NewPassword));
    }
}