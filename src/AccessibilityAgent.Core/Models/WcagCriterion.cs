namespace AccessibilityAgent.Core.Models;

public record WcagCriterion(
    string Id,
    string Number,
    string Name,
    WcagLevel Level,
    WcagVersion Version,
    string Principle,
    string Description,
    string HelpUrl)
{
    public static readonly WcagCriterion NonTextContent = new(
        "wcag111", "1.1.1", "Non-text Content", WcagLevel.A, WcagVersion.Wcag21,
        "Perceivable", "All non-text content that is presented to the user has a text alternative that serves the equivalent purpose.",
        "https://www.w3.org/WAI/WCAG21/Understanding/non-text-content.html");

    public static readonly WcagCriterion CaptionsPrerecorded = new(
        "wcag122", "1.2.2", "Captions (Prerecorded)", WcagLevel.A, WcagVersion.Wcag21,
        "Perceivable", "Captions are provided for all prerecorded audio content in synchronized media.",
        "https://www.w3.org/WAI/WCAG21/Understanding/captions-prerecorded.html");

    public static readonly WcagCriterion InfoAndRelationships = new(
        "wcag131", "1.3.1", "Info and Relationships", WcagLevel.A, WcagVersion.Wcag21,
        "Perceivable", "Information, structure, and relationships conveyed through presentation can be programmatically determined.",
        "https://www.w3.org/WAI/WCAG21/Understanding/info-and-relationships.html");

    public static readonly WcagCriterion IdentifyInputPurpose = new(
        "wcag135", "1.3.5", "Identify Input Purpose", WcagLevel.AA, WcagVersion.Wcag21,
        "Perceivable", "The purpose of each input field collecting information about the user can be programmatically determined.",
        "https://www.w3.org/WAI/WCAG21/Understanding/identify-input-purpose.html");

    public static readonly WcagCriterion ContrastMinimum = new(
        "wcag143", "1.4.3", "Contrast (Minimum)", WcagLevel.AA, WcagVersion.Wcag21,
        "Perceivable", "The visual presentation of text and images of text has a contrast ratio of at least 4.5:1 (or 3:1 for large text).",
        "https://www.w3.org/WAI/WCAG21/Understanding/contrast-minimum.html");

    public static readonly WcagCriterion ContrastEnhanced = new(
        "wcag146", "1.4.6", "Contrast (Enhanced)", WcagLevel.AAA, WcagVersion.Wcag21,
        "Perceivable", "The visual presentation of text and images of text has a contrast ratio of at least 7:1 (or 4.5:1 for large text).",
        "https://www.w3.org/WAI/WCAG21/Understanding/contrast-enhanced.html");

    public static readonly WcagCriterion NonTextContrast = new(
        "wcag1411", "1.4.11", "Non-text Contrast", WcagLevel.AA, WcagVersion.Wcag21,
        "Perceivable", "The visual presentation of user interface components and graphical objects has a contrast ratio of at least 3:1 against adjacent color(s).",
        "https://www.w3.org/WAI/WCAG21/Understanding/non-text-contrast.html");

    public static readonly WcagCriterion Keyboard = new(
        "wcag211", "2.1.1", "Keyboard", WcagLevel.A, WcagVersion.Wcag21,
        "Operable", "All functionality of the content is operable through a keyboard interface without requiring specific timings for individual keystrokes.",
        "https://www.w3.org/WAI/WCAG21/Understanding/keyboard.html");

    public static readonly WcagCriterion NoKeyboardTrap = new(
        "wcag212", "2.1.2", "No Keyboard Trap", WcagLevel.A, WcagVersion.Wcag21,
        "Operable", "If keyboard focus can be moved to a component of the page using a keyboard interface, then focus can be moved away from that component.",
        "https://www.w3.org/WAI/WCAG21/Understanding/no-keyboard-trap.html");

    public static readonly WcagCriterion BypassBlocks = new(
        "wcag241", "2.4.1", "Bypass Blocks", WcagLevel.A, WcagVersion.Wcag21,
        "Operable", "A mechanism is available to bypass blocks of content that are repeated on multiple Web pages (such as skip links).",
        "https://www.w3.org/WAI/WCAG21/Understanding/bypass-blocks.html");

    public static readonly WcagCriterion PageTitled = new(
        "wcag242", "2.4.2", "Page Titled", WcagLevel.A, WcagVersion.Wcag21,
        "Operable", "Web pages have titles that describe topic or purpose.",
        "https://www.w3.org/WAI/WCAG21/Understanding/page-titled.html");

    public static readonly WcagCriterion FocusOrder = new(
        "wcag243", "2.4.3", "Focus Order", WcagLevel.A, WcagVersion.Wcag21,
        "Operable", "If a Web page can be navigated sequentially and the navigation sequences affect meaning or operation, focusable components receive focus in an order that preserves meaning and operability.",
        "https://www.w3.org/WAI/WCAG21/Understanding/focus-order.html");

    public static readonly WcagCriterion LinkPurpose = new(
        "wcag244", "2.4.4", "Link Purpose (In Context)", WcagLevel.A, WcagVersion.Wcag21,
        "Operable", "The purpose of each link can be determined from the link text alone or from the link text together with its programmatically determined link context.",
        "https://www.w3.org/WAI/WCAG21/Understanding/link-purpose-in-context.html");

    public static readonly WcagCriterion HeadingsAndLabels = new(
        "wcag246", "2.4.6", "Headings and Labels", WcagLevel.AA, WcagVersion.Wcag21,
        "Operable", "Headings and labels describe topic or purpose.",
        "https://www.w3.org/WAI/WCAG21/Understanding/headings-and-labels.html");

    public static readonly WcagCriterion FocusVisible = new(
        "wcag247", "2.4.7", "Focus Visible", WcagLevel.AA, WcagVersion.Wcag21,
        "Operable", "Any keyboard operable user interface has a mode of operation where the keyboard focus indicator is visible.",
        "https://www.w3.org/WAI/WCAG21/Understanding/focus-visible.html");

    public static readonly WcagCriterion LanguageOfPage = new(
        "wcag311", "3.1.1", "Language of Page", WcagLevel.A, WcagVersion.Wcag21,
        "Understandable", "The default human language of each Web page can be programmatically determined.",
        "https://www.w3.org/WAI/WCAG21/Understanding/language-of-page.html");

    public static readonly WcagCriterion LabelsOrInstructions = new(
        "wcag332", "3.3.2", "Labels or Instructions", WcagLevel.A, WcagVersion.Wcag21,
        "Understandable", "Labels or instructions are provided when content requires user input.",
        "https://www.w3.org/WAI/WCAG21/Understanding/labels-or-instructions.html");

    public static readonly WcagCriterion NameRoleValue = new(
        "wcag412", "4.1.2", "Name, Role, Value", WcagLevel.A, WcagVersion.Wcag21,
        "Robust", "For all user interface components, the name and role can be programmatically determined; states, properties, and values can be set programmatically.",
        "https://www.w3.org/WAI/WCAG21/Understanding/name-role-value.html");
}
