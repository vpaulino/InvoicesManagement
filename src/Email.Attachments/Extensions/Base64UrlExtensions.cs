namespace ExtractLoadInvoices.Extensions;

/// <summary>
/// Base64 URL encoding extensions (internal utility)
/// </summary>
internal static class Base64UrlExtensions
{
    public static byte[] DecodeBase64Url(this string base64UrlString)
    {
        // Converting from RFC 4648 base64url to base64 encoding
        // see http://en.wikipedia.org/wiki/Base64#Implementations_and_history
        var base64 = base64UrlString.Replace('-', '+').Replace('_', '/');
        
        return Convert.FromBase64String(base64);
    }
}
