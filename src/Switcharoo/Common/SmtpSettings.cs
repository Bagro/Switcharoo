namespace Switcharoo.Common;

public sealed class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    
    public int Port { get; set; }
    
    public string Username { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
    
    public bool EnableSsl { get; set; }

    public string SenderName { get; set; } = string.Empty;
    
    public string Sender { get; set; } = string.Empty;
}
