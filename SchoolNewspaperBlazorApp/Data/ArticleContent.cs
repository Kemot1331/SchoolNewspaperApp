using System.Text;
using System.Text.RegularExpressions;

namespace SchoolNewspaperBlazorApp.Data;

// Treść artykułu zapisana jest w jednym polu Article.Text (bez zmian w bazie):
// akapity oddzielone pustą linią, a zdjęcie akapitu to osobny blok "[[img:ID]]" tuż po akapicie.
// Zdjęcie bez tekstu (samo w sobie) zapisujemy jako "[[fig:ID]]" – inaczej parser nie odróżniłby go
// od zdjęcia należącego do poprzedniego akapitu.
public static class ArticleContent
{
    public const int MaxTitleLength = 200;
    public const int MaxTextLength = 50_000;

    private static readonly Regex ImageToken =
        new(@"^\[\[(img|fig):(\d+)\]\]$", RegexOptions.Compiled);

    private static readonly Regex ImageTokenAnywhere =
        new(@"\[\[(?:img|fig):\d+\]\]", RegexOptions.Compiled);

    public sealed record Block(string Text, int? ImageFileId);

    public static List<Block> Parse(string? raw)
    {
        var result = new List<Block>();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return result;
        }

        var parts = raw.Replace("\r\n", "\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            var match = ImageToken.Match(trimmed);
            if (match.Success)
            {
                if (!int.TryParse(match.Groups[2].Value, out var fileId))
                {
                    // absurdalnie duży numer – to nie jest prawdziwe zdjęcie, pomijamy znacznik
                    continue;
                }

                var standalone = match.Groups[1].Value == "fig";
                if (!standalone && result.Count > 0 && result[^1].ImageFileId is null)
                {
                    // zdjęcie należy do poprzedniego akapitu
                    result[^1] = result[^1] with { ImageFileId = fileId };
                }
                else
                {
                    result.Add(new Block("", fileId));
                }
                continue;
            }

            result.Add(new Block(trimmed, null));
        }

        return result;
    }

    public static string Serialize(IEnumerable<Block> blocks)
    {
        var sb = new StringBuilder();
        foreach (var block in blocks)
        {
            // znacznika zdjęcia nie wolno wpisać ręcznie w treści – usuwamy go z tekstu
            var text = ImageTokenAnywhere.Replace(block.Text ?? "", "")
                .Replace("\r\n", "\n")
                .Trim();
            // pusta linia w środku akapitu rozbiłaby go przy odczycie
            text = Regex.Replace(text, @"\n{2,}", "\n");

            if (text.Length == 0 && block.ImageFileId is null)
            {
                continue;
            }

            if (text.Length > 0)
            {
                if (sb.Length > 0) sb.Append("\n\n");
                sb.Append(text);

                if (block.ImageFileId is int id)
                {
                    sb.Append("\n\n[[img:").Append(id).Append("]]");
                }
            }
            else if (block.ImageFileId is int figureId)
            {
                if (sb.Length > 0) sb.Append("\n\n");
                sb.Append("[[fig:").Append(figureId).Append("]]");
            }
        }

        return sb.ToString();
    }

    // Sam tekst, bez znaczników zdjęć – do kafelków na stronie głównej
    public static string PlainText(string? raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return "";
        }

        var withoutTokens = ImageTokenAnywhere.Replace(raw, "");
        return Regex.Replace(withoutTokens, @"\s*\n\s*", " ").Trim();
    }
}
