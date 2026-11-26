#nullable enable

namespace Wino.Core.Domain.Models.Mail;

/// <summary>Outcome of expanding Scriban macros in a template string.</summary>
public sealed class EmailTemplateExpandResult
{
    public required string Text { get; init; }
    public bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }

    public static EmailTemplateExpandResult Success(string text)
        => new() { Text = text, Succeeded = true };

    public static EmailTemplateExpandResult Failure(string original, string errorMessage)
        => new() { Text = original, Succeeded = false, ErrorMessage = errorMessage };
}
