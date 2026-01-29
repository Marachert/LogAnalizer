using LogAnalizer.Models;
using UglyToad.PdfPig;

namespace LogAnalizer.Services;

public static class PdfAnalyzer
{
    public static List<string> ExtractLines(string pdfPath)
    {
        var lines = new List<string>();
        using var document = PdfDocument.Open(pdfPath);
        foreach (var page in document.GetPages())
        {
            var text = page.Text;
            var pageLines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            lines.AddRange(pageLines);
        }

        return lines;
    }

    public static List<PdfAnalysisResult> Analyze(IEnumerable<InteractionRecord> interactions, List<string> pdfLines)
    {
        var results = new List<PdfAnalysisResult>();
        var index = 1;

        foreach (var interaction in interactions)
        {
            var match = FindEvidence(interaction, pdfLines);
            results.Add(new PdfAnalysisResult
            {
                Index = index,
                InteractionId = interaction.InteractionId,
                Status = match is null ? "Нет совпадений" : "Найдено совпадение",
                Evidence = match ?? ""
            });
            index++;
        }

        return results;
    }

    private static string? FindEvidence(InteractionRecord interaction, List<string> pdfLines)
    {
        var candidates = new[]
        {
            interaction.InteractionId,
            interaction.Date
        };

        foreach (var line in pdfLines)
        {
            foreach (var candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                if (line.Contains(candidate, StringComparison.OrdinalIgnoreCase))
                {
                    return line;
                }
            }
        }

        return null;
    }
}
