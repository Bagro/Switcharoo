namespace Switcharoo.Model;

public sealed record TeamEnvironment
{
    public Guid Id { get; init; } = Guid.Empty;
    
    public string Name { get; init; } = string.Empty;
}
