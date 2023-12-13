namespace Switcharoo.Model;

public sealed record TeamFeature
{
    public Guid Id { get; init; } = Guid.Empty;
    
    public string Name { get; init; } = string.Empty;
}
