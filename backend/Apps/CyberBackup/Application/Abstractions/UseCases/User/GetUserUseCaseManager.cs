using Application.Abstractions.Services.User.Contracts;
using Application.Abstractions.UseCases.User.Contracts;
using Domain.User;

namespace Application.Abstractions.UseCases.User;

/// <inheritdoc />
public sealed class GetUserUseCaseManager : IGetUserUseCaseManager
{
    private readonly IGetUserService _getUserService;

    public GetUserUseCaseManager(IGetUserService getUserService)
    {
        _getUserService = getUserService;
    }

    /// <inheritdoc />
    public async Task<UserModel?> Execute(Guid id, CancellationToken cancellationToken)
    {
        return await _getUserService.Get(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<UserModel>> Execute(CancellationToken cancellationToken)
    {
        return await _getUserService.GetAll(cancellationToken);
    }
}