namespace Frontend.Blazor;

public class AuthTokenStore
{
    public string? Token { get; private set; }

    public void SetToken(string token) => Token = token;

    public void Clear() => Token = null;
}

