using Infrastructure.Exceptions;
using Infrastructure.Exceptions.Base;

namespace Application.Features.Auth.Login;

public sealed class AccountDeactivatedException : BaseException
{
    public AccountDeactivatedException()
        : base("Ваш аккаунт деактивирован. Обратитесь к администратору.", ErrorCodes.AuthAccountDeactivated)
    {
    }
}
