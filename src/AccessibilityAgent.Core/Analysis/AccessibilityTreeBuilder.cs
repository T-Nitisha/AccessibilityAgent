using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Analysis;

public static class AccessibilityTreeBuilder
{
    public static AccessibilityNode BuildTree(IDocument document)
    {
        var root = new AccessibilityNode
        {
            Role = "document",
            Name = document.Title ?? "Document",
            Focusable = false
        };

        var body = document.Body;
        if (body != null)
        {
            foreach (var child in body.Children)
            {
                var childNode = ConvertElementToNode(child);
                if (childNode != null)
                {
                    root.Children.Add(childNode);
                }
            }
        }

        return root;
    }

    public static AccessibilityNode? ConvertElementToNode(IElement element)
    {
        // Skip script, style, meta, head
        var tag = element.TagName.ToLowerInvariant();
        if (tag is "script" or "style" or "noscript" or "template")
            return null;

        bool isHidden = element.HasAttribute("hidden") || 
                        element.GetAttribute("aria-hidden") == "true" ||
                        (element.GetAttribute("style")?.Contains("display:none", StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (element.GetAttribute("style")?.Contains("display: none", StringComparison.OrdinalIgnoreCase) ?? false);

        string role = DetermineRole(element);
        string? name = ComputeAccessibleName(element);

        // Check if decorative
        if (role == "presentation" || role == "none" || (tag == "img" && element.GetAttribute("alt") == ""))
        {
            // If decorative, we might still include children if any, or skip if empty
            if (element.Children.Length == 0) return null;
        }

        var node = new AccessibilityNode
        {
            Role = role,
            Name = name,
            Hidden = isHidden,
            Focusable = IsFocusable(element),
            Disabled = element.HasAttribute("disabled") || element.GetAttribute("aria-disabled") == "true"
        };

        // Capture states
        if (element.HasAttribute("aria-expanded"))
            node.States["expanded"] = element.GetAttribute("aria-expanded")!;
        if (element.HasAttribute("aria-checked"))
            node.States["checked"] = element.GetAttribute("aria-checked")!;
        if (element.HasAttribute("aria-selected"))
            node.States["selected"] = element.GetAttribute("aria-selected")!;
        if (element.HasAttribute("aria-invalid"))
            node.States["invalid"] = element.GetAttribute("aria-invalid")!;
        if (tag is "h1" or "h2" or "h3" or "h4" or "h5" or "h6")
            node.States["level"] = tag.Substring(1);

        foreach (var child in element.Children)
        {
            var childNode = ConvertElementToNode(child);
            if (childNode != null)
            {
                node.Children.Add(childNode);
            }
        }

        return node;
    }

    private static string DetermineRole(IElement element)
    {
        var explicitRole = element.GetAttribute("role");
        if (!string.IsNullOrWhiteSpace(explicitRole))
            return explicitRole.Trim().ToLowerInvariant();

        var tag = element.TagName.ToLowerInvariant();
        return tag switch
        {
            "a" when element.HasAttribute("href") => "link",
            "button" => "button",
            "input" => element.GetAttribute("type")?.ToLowerInvariant() switch
            {
                "checkbox" => "checkbox",
                "radio" => "radio",
                "submit" or "button" or "reset" => "button",
                "image" => "button",
                "search" => "searchbox",
                _ => "textbox"
            },
            "textarea" => "textbox",
            "select" => "combobox",
            "nav" => "navigation",
            "main" => "main",
            "header" => "banner",
            "footer" => "contentinfo",
            "aside" => "complementary",
            "section" when !string.IsNullOrWhiteSpace(ComputeAccessibleName(element)) => "region",
            "form" => "form",
            "table" => "table",
            "ul" or "ol" => "list",
            "li" => "listitem",
            "img" => "image",
            "h1" or "h2" or "h3" or "h4" or "h5" or "h6" => "heading",
            "dialog" => "dialog",
            _ => tag
        };
    }

    public static string? ComputeAccessibleName(IElement element)
    {
        // 1. aria-labelledby
        var labelledBy = element.GetAttribute("aria-labelledby");
        if (!string.IsNullOrWhiteSpace(labelledBy) && element.Owner != null)
        {
            var target = element.Owner.GetElementById(labelledBy.Trim());
            if (target != null && !string.IsNullOrWhiteSpace(target.TextContent))
                return target.TextContent.Trim();
        }

        // 2. aria-label
        var ariaLabel = element.GetAttribute("aria-label");
        if (!string.IsNullOrWhiteSpace(ariaLabel))
            return ariaLabel.Trim();

        var tag = element.TagName.ToLowerInvariant();

        // 3. alt for images
        if (tag == "img" && element.HasAttribute("alt"))
            return element.GetAttribute("alt")?.Trim();

        // 4. Form control labels
        if (tag is "input" or "select" or "textarea" && element.Owner != null)
        {
            var id = element.Id;
            if (!string.IsNullOrWhiteSpace(id))
            {
                var label = element.Owner.QuerySelector($"label[for='{id}']");
                if (label != null && !string.IsNullOrWhiteSpace(label.TextContent))
                    return label.TextContent.Trim();
            }

            // Or wrapped inside <label>
            var parentLabel = element.Closest("label");
            if (parentLabel != null && !string.IsNullOrWhiteSpace(parentLabel.TextContent))
                return parentLabel.TextContent.Trim();

            // placeholder or title fallback
            if (element.HasAttribute("placeholder"))
                return element.GetAttribute("placeholder")?.Trim();
        }

        // 5. title attribute
        var title = element.GetAttribute("title");
        if (!string.IsNullOrWhiteSpace(title))
            return title.Trim();

        // 6. Direct text for button, link, heading, summary
        if (tag is "button" or "a" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "summary" or "p" or "span")
        {
            var directText = element.TextContent?.Trim();
            if (!string.IsNullOrWhiteSpace(directText))
            {
                // Truncate long text
                return directText.Length > 80 ? directText.Substring(0, 77) + "..." : directText;
            }
        }

        return null;
    }

    private static bool IsFocusable(IElement element)
    {
        var tag = element.TagName.ToLowerInvariant();
        if (element.HasAttribute("tabindex"))
        {
            if (int.TryParse(element.GetAttribute("tabindex"), out var ti))
                return ti >= 0;
        }

        return tag switch
        {
            "a" when element.HasAttribute("href") => true,
            "button" => !element.HasAttribute("disabled"),
            "input" => !element.HasAttribute("disabled") && element.GetAttribute("type") != "hidden",
            "select" => !element.HasAttribute("disabled"),
            "textarea" => !element.HasAttribute("disabled"),
            "details" => true,
            _ => false
        };
    }
}
