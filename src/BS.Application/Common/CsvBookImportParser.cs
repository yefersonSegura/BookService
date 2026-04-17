using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BS.Application.DTOs;

namespace BS.Application.Common
{
    internal static class CsvBookImportParser
    {
        static CsvBookImportParser()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private static readonly Regex ScientificNotation = new(
            @"^\s*[\d]+[\.,]?[\d]*[Ee][+-]?\d+\s*$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public const string TemplateFileName = "plantilla_libros.csv";

        public static byte[] GetTemplateUtf8WithBom()
        {
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            var content =
                "isbn;title;publicationYear;authorName" + Environment.NewLine +
                "=\"9780306406157\";Ejemplo de titulo;1999;Ejemplo Autor" + Environment.NewLine;
            return utf8.GetBytes(content);
        }

        public static async Task<(bool Ok, List<CreateBookMassiveItemDto>? Items, string? ErrorMessage)> Parse(
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            await using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
            var bytes = buffer.ToArray();
            if (bytes.Length == 0)
            {
                return (false, null, "El archivo CSV está vacío o no tiene encabezados.");
            }

            var encoding = ChooseCsvEncoding(bytes);
            using var reader = new StreamReader(new MemoryStream(bytes), encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: false);

            var headerLine = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                return (false, null, "El archivo CSV está vacío o no tiene encabezados.");
            }

            var delimiter = DetectDelimiter(headerLine);
            var headers = SplitDelimitedLine(headerLine, delimiter).Select(static h => h.Trim().ToLowerInvariant()).ToArray();
            var idxIsbn = Array.IndexOf(headers, "isbn");
            var idxTitle = Array.IndexOf(headers, "title");
            var idxYear = Array.IndexOf(headers, "publicationyear");
            var idxAuthor = Array.IndexOf(headers, "authorname");

            if (idxIsbn < 0 || idxTitle < 0 || idxYear < 0 || idxAuthor < 0)
            {
                return (false, null, "El archivo debe tener columnas: isbn, title, publicationYear, authorName (separador ; o ,).");
            }

            var maxIdx = new[] { idxIsbn, idxTitle, idxYear, idxAuthor }.Max();
            var items = new List<CreateBookMassiveItemDto>();
            var lineNumber = 1;

            while (true)
            {
                var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (line == null)
                {
                    break;
                }

                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = SplitDelimitedLine(line, delimiter);
                if (parts.Count <= maxIdx)
                {
                    return (false, null, $"Línea {lineNumber}: faltan columnas (se esperaban al menos {maxIdx + 1} valores).");
                }

                if (!int.TryParse(parts[idxYear].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var year))
                {
                    return (false, null, $"Línea {lineNumber}: publicationYear no es un número entero válido.");
                }

                var rawIsbn = parts[idxIsbn];
                if (!TryNormalizeIsbnCell(rawIsbn, lineNumber, out var isbnNormalized, out var isbnError))
                {
                    return (false, null, isbnError);
                }

                items.Add(new CreateBookMassiveItemDto
                {
                    Isbn = isbnNormalized,
                    Title = parts[idxTitle].Trim(),
                    PublicationYear = year,
                    AuthorName = parts[idxAuthor].Trim()
                });
            }

            if (items.Count == 0)
            {
                return (false, null, "El CSV no contiene filas de datos.");
            }

            return (true, items, null);
        }

        /// <summary>
        /// Excel en español suele guardar CSV como Windows-1252 (ANSI). Si se lee como UTF-8 aparecen caracteres (U+FFFD).
        /// Con BOM UTF-8 o texto UTF-8 válido se mantiene UTF-8; si hay secuencias inválidas se reintenta con CP1252.
        /// </summary>
        private static Encoding ChooseCsvEncoding(byte[] bytes)
        {
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);
            }

            var asUtf8 = Encoding.UTF8.GetString(bytes);
            if (asUtf8.IndexOf('\uFFFD') < 0)
            {
                return Encoding.UTF8;
            }

            return Encoding.GetEncoding(1252);
        }

        /// <summary>Elige ; (típico Excel ES) o , según la primera línea.</summary>
        private static char DetectDelimiter(string firstLine)
        {
            var bySemi = SplitDelimitedLine(firstLine, ';');
            var byComma = SplitDelimitedLine(firstLine, ',');
            if (bySemi.Count >= 4 && byComma.Count < 4)
            {
                return ';';
            }

            if (byComma.Count >= 4 && bySemi.Count < 4)
            {
                return ',';
            }

            return bySemi.Count >= byComma.Count ? ';' : ',';
        }

        /// <summary>
        /// Limpia ISBN pegado desde Excel: fórmula ="…", apóstrofo forzando texto, notación científica (error).
        /// </summary>
        private static bool TryNormalizeIsbnCell(string raw, int lineNumber, out string isbn, out string? error)
        {
            isbn = string.Empty;
            error = null;
            if (string.IsNullOrWhiteSpace(raw))
            {
                error = $"Línea {lineNumber}: el ISBN está vacío.";
                return false;
            }

            var s = raw.Trim();

            if (ScientificNotation.IsMatch(s))
            {
                error =
                    $"Línea {lineNumber}: el ISBN aparece en notación científica (p. ej. 9,78E+12). En Excel seleccione la columna ISBN, " +
                    "Formato de celda → Texto, y escriba de nuevo el ISBN de 10 u 13 dígitos; o use la plantilla descargada desde la API.";
                return false;
            }

            // Campo entre comillas CSV
            if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
            {
                s = s[1..^1].Replace("\"\"", "\"");
            }

            // Excel: fórmula texto ="..."
            if (s.StartsWith("=", StringComparison.Ordinal))
            {
                s = s[1..].Trim();
                if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
                {
                    s = s[1..^1];
                }
                else if (s.Length >= 1 && s[0] == '\'')
                {
                    s = s[1..];
                }
            }
            else if (s.Length >= 1 && s[0] == '\'')
            {
                // Texto forzado en Excel
                s = s[1..];
            }

            isbn = IsbnText.Clean(s);
            if (string.IsNullOrEmpty(isbn))
            {
                error = $"Línea {lineNumber}: el ISBN no contiene dígitos válidos.";
                return false;
            }

            return true;
        }

        private static List<string> SplitDelimitedLine(string line, char delimiter)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            var inQuotes = false;
            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                        continue;
                    }

                    inQuotes = !inQuotes;
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }

            result.Add(sb.ToString().Trim());
            return result;
        }
    }
}
