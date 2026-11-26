#nullable enable

using Wino.Core.Domain.Models.Mail;

namespace Wino.Core.Domain.Interfaces;

/// <summary>Expands Scriban macros in email template subject and HTML body at insert time.</summary>
public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Renders <paramref name="source"/> with the given context.
    /// On parse or render failure, returns the original source and an error message.
    /// </summary>
    EmailTemplateExpandResult Expand(string? source, EmailTemplateRenderContext? context);
}
