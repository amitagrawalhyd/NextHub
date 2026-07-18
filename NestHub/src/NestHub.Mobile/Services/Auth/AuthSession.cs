namespace NestHub.Mobile.Services.Auth;

public sealed class AuthSession
{
    private const string TokenKey = "auth_token";
    private const string UserIdKey = "auth_user_id";
    private const string UserTypeKey = "auth_user_type";

    public string? Token { get; private set; }
    public Guid? UserId { get; private set; }
    public string? UserType { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public async Task RestoreAsync()
    {
        Token = await SecureStorage.Default.GetAsync(TokenKey);
        var userIdRaw = await SecureStorage.Default.GetAsync(UserIdKey);
        UserId = Guid.TryParse(userIdRaw, out var id) ? id : null;
        UserType = await SecureStorage.Default.GetAsync(UserTypeKey);
    }

    public async Task SignInAsync(string token, Guid userId, string userType)
    {
        Token = token;
        UserId = userId;
        UserType = userType;

        SecureStorage.Default.RemoveAll();
        await SecureStorage.Default.SetAsync(TokenKey, token);
        await SecureStorage.Default.SetAsync(UserIdKey, userId.ToString());
        await SecureStorage.Default.SetAsync(UserTypeKey, userType);
    }

    public void SignOut()
    {
        Token = null;
        UserId = null;
        UserType = null;
        SecureStorage.Default.RemoveAll();
    }
}
