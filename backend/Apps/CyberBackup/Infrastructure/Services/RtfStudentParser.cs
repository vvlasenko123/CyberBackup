using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Services;

/// <summary>
/// Парсер RTF-файлов со списком студентов (кодировка Windows-1251)
/// </summary>
public static class RtfStudentParser
{
    static RtfStudentParser()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// Парсит RTF-файл и возвращает список (ЛичныйНомер, ФИО)
    /// </summary>
    public static IReadOnlyList<(string PersonalNumber, string FullName)> Parse(Stream stream)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var win1251 = Encoding.GetEncoding(1251);

        using var reader = new StreamReader(stream, win1251, detectEncodingFromByteOrderMarks: false);
        var rtf = reader.ReadToEnd();

        var decoded = Regex.Replace(rtf, @"\\'([0-9a-fA-F]{2})", m =>
        {
            var b = Convert.ToByte(m.Groups[1].Value, 16);
            return win1251.GetString(new[] { b });
        });

        var rows = decoded.Split(@"\row", StringSplitOptions.RemoveEmptyEntries);
        var results = new List<(string, string)>();

        foreach (var row in rows)
        {
            var cells = Regex.Split(row, @"\\cell(?![a-zA-Z])")
                .Select(CleanCell)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToList();

            if (cells.Count < 3) continue;

            var personalNumber = cells[1];
            var fullName = cells[2];

            if (personalNumber.Contains("номер", StringComparison.OrdinalIgnoreCase) ||
                fullName.Contains("ФИО", StringComparison.OrdinalIgnoreCase) ||
                !Regex.IsMatch(personalNumber, @"^\d+$"))
                continue;

            if (!string.IsNullOrWhiteSpace(personalNumber) && !string.IsNullOrWhiteSpace(fullName))
                results.Add((personalNumber.Trim(), fullName.Trim()));
        }

        return results;
    }

    private static string CleanCell(string cell)
    {
        var cleaned = Regex.Replace(cell, @"\\[a-z*]+\d*\s?", " ");
        cleaned = Regex.Replace(cleaned, @"[{}]", "");
        cleaned = Regex.Replace(cleaned, @"\s+", " ");
        return cleaned.Trim();
    }
}
