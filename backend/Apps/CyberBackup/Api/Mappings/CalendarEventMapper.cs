using Api.Controllers.Models.Request;
using Api.Controllers.Models.Response;
using Application.DTO.Calendar;
using AutoMapper;
using Domain.Calendar;

namespace Api.Mappings;

/// <summary>
/// Маппер событий календаря.
/// </summary>
public sealed class CalendarEventMapper : Profile
{
    public CalendarEventMapper()
    {
        CreateMap<CreateCalendarEventRequest, CalendarEventDto>()
            .ForMember(x => x.Title, y => y.MapFrom(z => z.Title))
            .ForMember(x => x.EventType, y => y.MapFrom(z => z.EventType))
            .ForMember(x => x.StartsAtUtc, y => y.MapFrom(z => z.StartsAtUtc))
            .ForMember(x => x.EndsAtUtc, y => y.MapFrom(z => z.EndsAtUtc))
            .ForMember(x => x.NotifyAtUtc, y => y.MapFrom(z => z.NotifyAtUtc));

        CreateMap<CalendarEventModel, CalendarEventResponse>()
            .ForMember(x => x.Id, y => y.MapFrom(z => z.Id))
            .ForMember(x => x.UserId, y => y.MapFrom(z => z.UserId))
            .ForMember(x => x.Title, y => y.MapFrom(z => z.Title))
            .ForMember(x => x.EventType, y => y.MapFrom(z => z.EventType))
            .ForMember(x => x.Status, y => y.MapFrom(z => z.Status))
            .ForMember(x => x.StartsAtUtc, y => y.MapFrom(z => z.StartsAtUtc))
            .ForMember(x => x.EndsAtUtc, y => y.MapFrom(z => z.EndsAtUtc))
            .ForMember(x => x.NotifyAtUtc, y => y.MapFrom(z => z.NotifyAtUtc))
            .ForMember(x => x.NotifiedAtUtc, y => y.MapFrom(z => z.NotifiedAtUtc))
            .ForMember(x => x.CreatedAtUtc, y => y.MapFrom(z => z.CreatedAtUtc))
            .ForMember(x => x.UpdatedAtUtc, y => y.MapFrom(z => z.UpdatedAtUtc));
    }
}