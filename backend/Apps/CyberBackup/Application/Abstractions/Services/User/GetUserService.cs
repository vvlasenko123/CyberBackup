using Application.Abstractions.Services.User.Contracts;
using Domain.Repositories;
using Domain.User;

namespace Application.Abstractions.Services.User;

/// <inheritdoc />
public sealed class GetUserService : IGetUserService
{
    private readonly IUserRepository _userRepository;

    public GetUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<UserModel?> Get(Guid request, CancellationToken cancellationToken)
    {
        return await _userRepository.GetByIdAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<UserModel>> GetAll(CancellationToken cancellationToken)
    {
        return await _userRepository.GetAllAsync(cancellationToken);
    }
}