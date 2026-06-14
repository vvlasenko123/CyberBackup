namespace Application.Features.Auth.Login;

public sealed class AccountDeactivatedException : Exception
{
    public AccountDeactivatedException()
        : base("Ваш аккаунт деактивирован. Обратитесь к администратору.")
    {
    }
}
