namespace Switcharoo.Model;

public sealed record TeamMember
{
    public Guid Id { get; init; } = Guid.Empty;
    
    public string Name { get; init; } = string.Empty;
    
    public string Email { get; init; } = string.Empty;
}
