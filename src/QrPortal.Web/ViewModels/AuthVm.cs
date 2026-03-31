namespace QrPortal.Web.ViewModels;

public class LoginVm
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Error { get; set; }
}
