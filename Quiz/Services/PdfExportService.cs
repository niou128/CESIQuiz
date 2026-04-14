using Microsoft.Maui.Storage;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Quiz.Models;

namespace Quiz.Services;

public sealed class PdfExportService : IPdfExportService
{
    public Task<string> ExportScoresAsync(UserAccount user, IReadOnlyList<QuizScore> scores)
    {
        if (scores.Count == 0)
        {
            throw new InvalidOperationException("Aucun score à exporter.");
        }

        var exportDirectory = ResolveExportDirectory();
        Directory.CreateDirectory(exportDirectory);

        var fileName = $"scores-{Sanitize(user.Username)}-{DateTime.Now:yyyyMMdd-HHmmss}.pdf";
        var filePath = Path.Combine(exportDirectory, fileName);

        using var document = new PdfDocument();
        document.Info.Title = $"Scores de {user.Username}";

        var titleFont = new XFont("Arial", 18, XFontStyleEx.Bold);
        var bodyFont = new XFont("Arial", 11, XFontStyleEx.Regular);
        var headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);

        PdfPage page = document.AddPage();
        page.Size = PageSize.A4;
        var graphics = XGraphics.FromPdfPage(page);
        double y = 40;

        graphics.DrawString($"Historique des scores - {user.Username}", titleFont, XBrushes.Black, new XRect(40, y, page.Width - 80, 24), XStringFormats.TopLeft);
        y += 30;
        graphics.DrawString($"Export généré le {DateTime.Now:dd/MM/yyyy HH:mm}", bodyFont, XBrushes.Black, new XRect(40, y, page.Width - 80, 18), XStringFormats.TopLeft);
        y += 30;

        graphics.DrawString("Date", headerFont, XBrushes.Black, new XRect(40, y, 130, 18), XStringFormats.TopLeft);
        graphics.DrawString("Mode", headerFont, XBrushes.Black, new XRect(180, y, 90, 18), XStringFormats.TopLeft);
        graphics.DrawString("Questions", headerFont, XBrushes.Black, new XRect(280, y, 90, 18), XStringFormats.TopLeft);
        graphics.DrawString("Score", headerFont, XBrushes.Black, new XRect(380, y, 90, 18), XStringFormats.TopLeft);
        graphics.DrawString("%", headerFont, XBrushes.Black, new XRect(470, y, 60, 18), XStringFormats.TopLeft);
        y += 20;

        foreach (var score in scores)
        {
            if (y > page.Height - 40)
            {
                page = document.AddPage();
                page.Size = PageSize.A4;
                graphics = XGraphics.FromPdfPage(page);
                y = 40;
            }

            graphics.DrawString(score.CompletedAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm"), bodyFont, XBrushes.Black, new XRect(40, y, 130, 18), XStringFormats.TopLeft);
            graphics.DrawString(score.QuizMode == QuizSourceMode.Local ? "Local" : "Distant", bodyFont, XBrushes.Black, new XRect(180, y, 90, 18), XStringFormats.TopLeft);
            graphics.DrawString(score.QuestionCount.ToString(), bodyFont, XBrushes.Black, new XRect(280, y, 90, 18), XStringFormats.TopLeft);
            graphics.DrawString($"{score.CorrectAnswers}/{score.QuestionCount}", bodyFont, XBrushes.Black, new XRect(380, y, 90, 18), XStringFormats.TopLeft);
            graphics.DrawString(score.Percentage.ToString(), bodyFont, XBrushes.Black, new XRect(470, y, 60, 18), XStringFormats.TopLeft);
            y += 18;
        }

        document.Save(filePath);
        return Task.FromResult(filePath);
    }

    private static string ResolveExportDirectory()
    {
        var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return string.IsNullOrWhiteSpace(documentsDirectory)
            ? Path.Combine(FileSystem.Current.AppDataDirectory, "exports")
            : Path.Combine(documentsDirectory, "QuizExports");
    }

    private static string Sanitize(string value)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidChar, '-');
        }

        return value;
    }
}
