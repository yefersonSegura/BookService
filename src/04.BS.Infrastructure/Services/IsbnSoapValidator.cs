using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using BS.Application.Common;
using BS.Application.Interfaces;

namespace _04.BS.Infrastructure.Services
{
    internal sealed class IsbnSoapValidator : IIsbnSoapValidator
    {
        private static readonly XNamespace Ns = "http://webservices.daehosting.com/ISBN";
        private readonly HttpClient _httpClient;

        public IsbnSoapValidator(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> IsValid(string isbn, CancellationToken cancellationToken = default)
        {
            var cleaned = IsbnText.Clean(isbn);
            if (cleaned.Length == 13 && cleaned.All(char.IsDigit))
            {
                return await CallSoap("IsValidISBN13", cleaned, "IsValidISBN13Result", cancellationToken).ConfigureAwait(false);
            }

            if (cleaned.Length == 10 && IsbnText.IsIsbn10Shape(cleaned))
            {
                return await CallSoap("IsValidISBN10", cleaned, "IsValidISBN10Result", cancellationToken).ConfigureAwait(false);
            }

            return false;
        }

        private async Task<bool> CallSoap(string operation, string sIsbn, string resultElement, CancellationToken cancellationToken)
        {
            var nsUri = Ns.NamespaceName;
            var envelope =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"" + nsUri + "\">" +
                "<soap:Body>" +
                "<tns:" + operation + "><tns:sISBN>" + sIsbn + "</tns:sISBN></tns:" + operation + ">" +
                "</soap:Body></soap:Envelope>";

            using var content = new StringContent(envelope, Encoding.UTF8, "text/xml");
            content.Headers.ContentType = new MediaTypeHeaderValue("text/xml") { CharSet = "utf-8" };

            using var request = new HttpRequestMessage(HttpMethod.Post, "isbnservice.wso")
            {
                Content = content
            };
            request.Headers.TryAddWithoutValidation("SOAPAction", "\"\"");

            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var xml = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var doc = XDocument.Parse(xml);
            var el = doc.Descendants(Ns + resultElement).FirstOrDefault();
            return el != null && bool.TryParse(el.Value, out var ok) && ok;
        }
    }
}
