using OwlSight.Core.Review;
using Spectre.Console;

namespace OwlSight.Cli.Output;

public sealed class ConsoleOutputWriter : IReviewOutputWriter
{
    public Task WriteAsync(ReviewResult result, CancellationToken ct = default)
    {
        AnsiConsole.WriteLine();

        if (result.Findings.Count == 0)
        {
            AnsiConsole.MarkupLine("[green bold]No issues found![/]");
            WriteSummaryTable(result.Summary);
            return Task.CompletedTask;
        }

        // Group findings by file
        var byFile = result.Findings
            .GroupBy(f => f.File)
            .OrderBy(g => g.Key);

        foreach (var group in byFile)
        {
            AnsiConsole.MarkupLine($"\n[bold underline]{Markup.Escape(group.Key)}[/]");

            foreach (var finding in group.OrderByDescending(f => f.Severity))
            {
                ct.ThrowIfCancellationRequested();
                WriteFinding(finding);
            }
        }

        AnsiConsole.WriteLine();
        WriteSummaryTable(result.Summary);

        if (result.HasCriticalFindings)
        {
            AnsiConsole.MarkupLine("\n[red bold]Review FAILED — critical issues found.[/]");
        }

        return Task.CompletedTask;
    }

    private static void WriteFinding(ReviewFinding finding)
    {
        var severityMarkup = finding.Severity switch
        {
            ReviewSeverity.Critical => "[red bold]CRITICAL[/]",
            ReviewSeverity.Warning => "[yellow bold]WARNING[/]",
            ReviewSeverity.Info => "[blue]INFO[/]",
            ReviewSeverity.Nitpick => "[grey]NITPICK[/]",
            _ => finding.Severity.ToString()
        };

        var location = finding.Line.HasValue
            ? finding.EndLine.HasValue
                ? $":{finding.Line}-{finding.EndLine}"
                : $":{finding.Line}"
            : string.Empty;

        AnsiConsole.MarkupLine($"  {severityMarkup} {Markup.Escape(finding.Title)} [dim]({Markup.Escape(finding.File)}{location})[/]");
        AnsiConsole.MarkupLine($"    [dim]{Markup.Escape(finding.Description)}[/]");

        if (!string.IsNullOrEmpty(finding.Suggestion))
        {
            AnsiConsole.MarkupLine($"    [green]Suggestion:[/] {Markup.Escape(finding.Suggestion)}");
        }

        if (!string.IsNullOrEmpty(finding.RuleId))
        {
            AnsiConsole.MarkupLine($"    [dim]Rule: {Markup.Escape(finding.RuleId)}[/]");
        }

        AnsiConsole.WriteLine();
    }

    private static void WriteSummaryTable(ReviewSummary summary)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Review Summary[/]")
            .AddColumn("Metric")
            .AddColumn("Value");

        table.AddRow("Files Reviewed", summary.ReviewedFilesCount.ToString());
        table.AddRow("Total Findings", summary.TotalFindings.ToString());
        table.AddRow("Batches", summary.BatchCount.ToString());

        if (summary.BySeverity.TryGetValue(ReviewSeverity.Critical, out var critical))
            table.AddRow("[red]Critical[/]", critical.ToString());
        if (summary.BySeverity.TryGetValue(ReviewSeverity.Warning, out var warning))
            table.AddRow("[yellow]Warning[/]", warning.ToString());
        if (summary.BySeverity.TryGetValue(ReviewSeverity.Info, out var info))
            table.AddRow("[blue]Info[/]", info.ToString());
        if (summary.BySeverity.TryGetValue(ReviewSeverity.Nitpick, out var nitpick))
            table.AddRow("[grey]Nitpick[/]", nitpick.ToString());

        AnsiConsole.Write(table);
    }
}
