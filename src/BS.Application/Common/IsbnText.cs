using System.Text;

namespace BS.Application.Common
{
    internal static class IsbnText
    {
        public static string Clean(string? isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(isbn.Length);
            foreach (var c in isbn.Trim().ToUpperInvariant())
            {
                if (char.IsDigit(c) || c == 'X')
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static bool IsIsbn10Shape(string s)
        {
            if (s.Length != 10)
            {
                return false;
            }

            for (var i = 0; i < 9; i++)
            {
                if (!char.IsDigit(s[i]))
                {
                    return false;
                }
            }

            var last = s[9];
            return char.IsDigit(last) || last == 'X';
        }
    }
}
