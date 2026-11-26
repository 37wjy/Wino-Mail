#nullable enable

using System;
using System.Collections.Generic;

namespace Wino.Core.Domain.Models.Mail;

/// <summary>Values exposed to Scriban when a template is inserted in the composer.</summary>
public sealed class EmailTemplateRenderContext
{
    public static EmailTemplateRenderContext Empty { get; } = new();

    public string AccountName { get; init; } = string.Empty;
    public string AccountAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public IReadOnlyList<EmailTemplateRecipientModel> To { get; init; } = Array.Empty<EmailTemplateRecipientModel>();
    public IReadOnlyList<EmailTemplateRecipientModel> Cc { get; init; } = Array.Empty<EmailTemplateRecipientModel>();
    public IReadOnlyList<EmailTemplateRecipientModel> Bcc { get; init; } = Array.Empty<EmailTemplateRecipientModel>();
    public DayOfWeek FirstDayOfWeek { get; init; } = DayOfWeek.Monday;
    public DateTime Now { get; init; } = DateTime.Now;
}

public sealed class EmailTemplateRecipientModel
{
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
}
