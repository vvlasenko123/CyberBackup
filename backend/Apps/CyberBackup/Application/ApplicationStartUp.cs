using Application.Abstractions.Services.Calendar;
using Application.Abstractions.Services.Calendar.Contracts;
using Application.Abstractions.Services.Groups;
using Application.Abstractions.Services.Groups.Contracts;
using Application.Abstractions.Services.User;
using Application.Abstractions.Services.User.Contracts;
using Application.Abstractions.Services.Laboratories;
using Application.Abstractions.Services.Laboratories.Contracts;
using Application.Abstractions.Services.Posts;
using Application.Abstractions.Services.Posts.Contracts;
using Application.Abstractions.Services.Questions;
using Application.Abstractions.Services.Questions.Contracts;
using Application.Abstractions.UseCases.Auth.Contracts;
using Application.Abstractions.UseCases.Auth.Register;
using Application.Abstractions.UseCases.Calendar;
using Application.Abstractions.UseCases.Calendar.Contracts;
using Application.Abstractions.UseCases.User;
using Application.Abstractions.UseCases.User.Contracts;
using Application.Features.Auth.Login;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Extension Application слоя
/// </summary>
public static class ApplicationStartUp
{
    /// <summary>
    /// Подключение Application слоя
    /// </summary>
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateUserUseCaseManager, CreateUserUseCaseManager>();
        services.AddScoped<IUpdateUserUseCaseManager, UpdateUserUseCaseManager>();
        services.AddScoped<IDeleteUserUseCaseManager, DeleteUserUseCaseManager>();
        services.AddScoped<IGetUserUseCaseManager, GetUserUseCaseManager>();
        services.AddScoped<IChangePasswordUseCaseManager, ChangePasswordUseCaseManager>();

        services.AddScoped<ICreateUserService, CreateUserService>();
        services.AddScoped<IUpdateUserService, UpdateUserService>();
        services.AddScoped<IDeleteUserService, DeleteUserService>();
        services.AddScoped<IGetUserService, GetUserService>();

        services.AddScoped<ICreateCalendarEventUseCaseManager, CreateCalendarEventUseCaseManager>();
        services.AddScoped<IGetCalendarEventsUseCaseManager, GetCalendarEventsUseCaseManager>();

        services.AddScoped<ICalendarEventService, CalendarEventService>();
        services.AddHostedService<CalendarNotificationHostedService>();

        services.AddScoped<IRegisterUseCaseManager, RegisterUseCaseManager>();
        services.AddScoped<ILoginUseCaseManager, LoginUseCaseManager>();
        services.AddScoped<ILaboratoryService, LaboratoryService>();
        services.AddScoped<ILaboratoryFlagHashService, LaboratoryFlagHashService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IQuestionService, QuestionService>();
    }
}
