using Api.Controllers.Models.Request;
using Application.DTO.Auth;
using AutoMapper;

namespace Api.Mappings;

/// <summary>
/// Маппер для auth.
/// </summary>
public class AuthMapper : Profile
{
    public AuthMapper()
    {
        CreateMap<RegisterRequest, RegisterRequestDto>()
            .ForCtorParam(nameof(RegisterRequestDto.Email), y => y.MapFrom(z => z.Email))
            .ForCtorParam(nameof(RegisterRequestDto.FullName), y => y.MapFrom(z => z.FullName))
            .ForCtorParam(nameof(RegisterRequestDto.Password), y => y.MapFrom(z => z.Password));
        
        CreateMap<LoginRequest, LoginRequestDto>()
            .ForCtorParam(nameof(LoginRequestDto.Email), y => y.MapFrom(z => z.Email))
            .ForCtorParam(nameof(LoginRequestDto.Password), y => y.MapFrom(z => z.Password));
    }
}