using Application.Abstractions.Services.UserProfiles;
using Application.DTO.UserProfiles;
using Tests.Support;

namespace Tests;

public sealed class ProfileTests
{
    private static string? Phone(string? value) => ContactNormalizer.NormalizePhone(value)?.Value;

    private static string? Telegram(string? value) => ContactNormalizer.NormalizeTelegram(value);

    private static string? Vk(string? value) => ContactNormalizer.NormalizeVk(value);

    // ---------- телефон ----------

    [Theory]
    [InlineData("+7 (900) 123-45-67", "+79001234567")]
    [InlineData("8 900 123 45 67", "89001234567")]
    [InlineData("  +1-202-555-0143  ", "+12025550143")]
    [InlineData("9001234567", "9001234567")]
    [InlineData("+123456789012345", "+123456789012345")]
    public void Phone_is_normalized_to_optional_plus_and_digits(string input, string expected)
    {
        Assert.Equal(expected, Phone(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_values_clear_the_contact(string? input)
    {
        Assert.Null(Phone(input));
        Assert.Null(Telegram(input));
        Assert.Null(Vk(input));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("123456789")]
    [InlineData("+1234567890123456")]
    [InlineData("phone: 9001234567")]
    [InlineData("900+1234567")]
    [InlineData("+")]
    [InlineData("++79001234567")]
    [InlineData("9001234567x")]
    public void Invalid_phone_is_rejected(string input)
    {
        var exception = Assert.Throws<ProfileException>(() => Phone(input));

        Assert.Equal(ProfileException.PhoneInvalid, exception.Code);
    }

    [Fact]
    public void Overlong_phone_input_is_rejected()
    {
        Assert.Throws<ProfileException>(() => Phone(new string('1', 201)));
    }

    // ---------- Telegram ----------

    [Theory]
    [InlineData("durov", "durov")]
    [InlineData("@durov_official", "durov_official")]
    [InlineData("https://t.me/durov", "durov")]
    [InlineData("t.me/durov/", "durov")]
    [InlineData("HTTP://TELEGRAM.ME/Some_User", "Some_User")]
    [InlineData("  @user_1  ", "user_1")]
    public void Telegram_is_stripped_to_the_username(string input, string expected)
    {
        Assert.Equal(expected, Telegram(input));
    }

    [Theory]
    [InlineData("abcd")]
    [InlineData("1user")]
    [InlineData("_user")]
    [InlineData("user name")]
    [InlineData("user-name")]
    [InlineData("пользователь")]
    [InlineData("https://example.com/durov")]
    [InlineData("@@durov")]
    public void Invalid_telegram_is_rejected(string input)
    {
        var exception = Assert.Throws<ProfileException>(() => Telegram(input));

        Assert.Equal(ProfileException.TelegramInvalid, exception.Code);
    }

    [Fact]
    public void Telegram_username_length_limits()
    {
        Assert.Equal("a" + new string('b', 31), Telegram("a" + new string('b', 31)));
        Assert.Throws<ProfileException>(() => Telegram("a" + new string('b', 32)));
    }

    // ---------- VK ----------

    [Theory]
    [InlineData("durov", "durov")]
    [InlineData("id12345", "id12345")]
    [InlineData("https://vk.com/durov", "durov")]
    [InlineData("https://m.vk.com/id12345/", "id12345")]
    [InlineData("vk.com/some.user_1", "some.user_1")]
    [InlineData("HTTPS://WWW.VK.COM/Durov", "Durov")]
    public void Vk_is_stripped_to_the_handle(string input, string expected)
    {
        Assert.Equal(expected, Vk(input));
    }

    [Theory]
    [InlineData("abcd")]
    [InlineData("user name")]
    [InlineData("user-name")]
    [InlineData("@durov")]
    [InlineData("https://example.com/durov")]
    [InlineData("https://vk.com/")]
    public void Invalid_vk_is_rejected(string input)
    {
        var exception = Assert.Throws<ProfileException>(() => Vk(input));

        Assert.Equal(ProfileException.VkInvalid, exception.Code);
    }

    // ---------- сервис профиля ----------

    [Fact]
    public async Task Update_replaces_all_three_contacts_and_returns_the_profile()
    {
        var repository = new FakeUserRepository();
        var userId = repository.AddUser();
        var service = new UserProfileService(repository);

        var profile = await service.UpdateContactsAsync(
            userId,
            new UpdateContactsDto { Phone = "+7 900 123 45 67", Telegram = "@user_1", Vk = "https://vk.com/user1" },
            CancellationToken.None);

        Assert.Equal("+79001234567", profile.Phone?.Value);
        Assert.Equal("user_1", profile.Telegram);
        Assert.Equal("user1", profile.Vk);
        Assert.Equal(("+79001234567", "user_1", "user1"), repository.ContactsOf(userId));
    }

    [Fact]
    public async Task Update_is_a_full_replacement_so_omitted_fields_are_cleared()
    {
        var repository = new FakeUserRepository();
        var userId = repository.AddUser();
        var service = new UserProfileService(repository);

        await service.UpdateContactsAsync(
            userId,
            new UpdateContactsDto { Phone = "9001234567", Telegram = "user_1", Vk = "user1" },
            CancellationToken.None);
        var profile = await service.UpdateContactsAsync(
            userId,
            new UpdateContactsDto { Telegram = "other_1" },
            CancellationToken.None);

        Assert.Null(profile.Phone);
        Assert.Equal("other_1", profile.Telegram);
        Assert.Null(profile.Vk);
    }

    [Fact]
    public async Task Invalid_contact_changes_nothing()
    {
        var repository = new FakeUserRepository();
        var userId = repository.AddUser();
        var service = new UserProfileService(repository);

        await service.UpdateContactsAsync(userId, new UpdateContactsDto { Phone = "9001234567" }, CancellationToken.None);

        await Assert.ThrowsAsync<ProfileException>(() => service.UpdateContactsAsync(
            userId,
            new UpdateContactsDto { Phone = "9007654321", Telegram = "x" },
            CancellationToken.None));

        Assert.Equal(("9001234567", null, null), repository.ContactsOf(userId));
    }

    [Fact]
    public async Task Missing_user_is_not_found()
    {
        var service = new UserProfileService(new FakeUserRepository());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetAsync(Guid.NewGuid(), CancellationToken.None));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateContactsAsync(
            Guid.NewGuid(),
            new UpdateContactsDto { Phone = "9001234567" },
            CancellationToken.None));
    }
}
