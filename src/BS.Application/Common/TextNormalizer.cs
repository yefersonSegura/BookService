using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BS.Application.Common
{
    internal static class TextNormalizer
    {
        public static string NormalizeForPersistence(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var upper = value.Trim().ToUpperInvariant();
            var withoutDigits = new StringBuilder(upper.Length);
            foreach (var c in upper)
            {
                if (!char.IsDigit(c))
                {
                    withoutDigits.Append(c);
                }
            }

            var normalized = withoutDigits.ToString().Normalize(NormalizationForm.FormD);
            var folded = new StringBuilder(normalized.Length);
            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    folded.Append(c);
                }
            }

            var collapsed = Regex.Replace(folded.ToString().Normalize(NormalizationForm.FormC).Trim(), @"\s+", " ");
            return collapsed;
        }
    }
}
