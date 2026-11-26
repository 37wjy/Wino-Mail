#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Scriban;
using Scriban.Runtime;
using Wino.Core.Domain.Extensions;
using Wino.Core.Domain.Interfaces;
using Wino.Core.Domain.Models.Mail;

namespace Wino.Services;

/// <summary>
/// Expands Scriban templates through <see cref="ScriptObject"/> only (Native AOT safe).
/// No template loader, so include/import cannot read files.
/// </summary>
public sealed partial class ScribanEmailTemplateRenderer : IEmailTemplateRenderer
{
    public EmailTemplateExpandResult Expand(string? source, EmailTemplateRenderContext? context)
    {
        if (string.IsNullOrEmpty(source))
            return EmailTemplateExpandResult.Success(source ?? string.Empty);

        // The HTML editor stores quotes in macros as entities (&#39;) or curly quotes; decode only
        // inside Scriban spans so the rest of the HTML is untouched.
        var prepared = PrepareHtmlSource(source);

        var template = Template.Parse(prepared);
        if (template.HasErrors)
        {
            var message = string.Join("; ", template.Messages.Select(static m => m.Message));
            return EmailTemplateExpandResult.Failure(source, message);
        }

        try
        {
            var globals = BuildGlobals(context ?? EmailTemplateRenderContext.Empty);
            var templateContext = new TemplateContext();
            templateContext.PushGlobal(globals);
            var rendered = template.Render(templateContext);
            return EmailTemplateExpandResult.Success(rendered);
        }
        catch (Exception ex)
        {
            return EmailTemplateExpandResult.Failure(source, ex.Message);
        }
    }

    private static string PrepareHtmlSource(string source)
        => ScribanSpanRegex().Replace(source, static match => NormalizeScribanSpan(match.Value));

    private static string NormalizeScribanSpan(string span)
    {
        var decoded = WebUtility.HtmlDecode(span);
        return decoded
            .Replace('\u2018', '\'')
            .Replace('\u2019', '\'')
            .Replace('\u201C', '"')
            .Replace('\u201D', '"');
    }

    [GeneratedRegex(@"\{\{[\s\S]*?\}\}|\{%[\s\S]*?%\}", RegexOptions.CultureInvariant)]
    private static partial Regex ScribanSpanRegex();

    private static ScriptObject BuildGlobals(EmailTemplateRenderContext context)
    {
        var now = context.Now;
        var weekStart = now.GetWeekStartDateForDate(context.FirstDayOfWeek);
        var weekEnd = weekStart.AddDays(6);
        var culture = CultureInfo.CurrentCulture;

        var globals = new ScriptObject
        {
            ["account"] = new ScriptObject
            {
                ["name"] = context.AccountName ?? string.Empty,
                ["address"] = context.AccountAddress ?? string.Empty,
            },
            ["from"] = new ScriptObject
            {
                ["name"] = context.FromName ?? string.Empty,
                ["address"] = context.FromAddress ?? string.Empty,
            },
            ["to"] = ToRecipientArray(context.To),
            ["cc"] = ToRecipientArray(context.Cc),
            ["bcc"] = ToRecipientArray(context.Bcc),
            ["week_start"] = weekStart.ToString("d", culture),
            ["week_end"] = weekEnd.ToString("d", culture),
            ["week_range"] = weekStart.ToString("d", culture) + " – " + weekEnd.ToString("d", culture),
        };

        return globals;
    }

    private static ScriptArray ToRecipientArray(IReadOnlyList<EmailTemplateRecipientModel>? recipients)
    {
        var array = new ScriptArray();
        if (recipients == null)
            return array;

        foreach (var recipient in recipients)
        {
            array.Add(new ScriptObject
            {
                ["name"] = recipient.Name ?? string.Empty,
                ["address"] = recipient.Address ?? string.Empty,
            });
        }

        return array;
    }
}
