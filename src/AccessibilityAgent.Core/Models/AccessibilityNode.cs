namespace AccessibilityAgent.Core.Models;

public class AccessibilityNode
{
    public required string Role { get; set; }
    public string? Name { get; set; }
    public string? Value { get; set; }
    public string? Description { get; set; }
    public bool Focusable { get; set; }
    public bool Disabled { get; set; }
    public bool Hidden { get; set; }
    public Dictionary<string, string> States { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<AccessibilityNode> Children { get; set; } = new();

    public override string ToString()
    {
        var namePart = string.IsNullOrWhiteSpace(Name) ? "" : $" \"{Name}\"";
        var rolePart = Role;
        var statesList = States.Where(s => !string.IsNullOrWhiteSpace(s.Value)).Select(s => $"{s.Key}={s.Value}");
        var statePart = statesList.Any() ? $" [{string.Join(", ", statesList)}]" : "";
        return $"{rolePart}{namePart}{statePart}";
    }
}
