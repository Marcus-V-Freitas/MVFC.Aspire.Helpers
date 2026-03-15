namespace MVFC.Aspire.Helpers.Tests.Integration.Extensions;

internal static class HttpTestExtensions 
{
    internal static async ValueTask<HttpClient> CreateHttpClient(this ProjectAppHost app, string resourceName)
    {
        var uri = await app.GetConnectionString(resourceName).ConfigureAwait(false);

        return new HttpClient()
        {
            BaseAddress = new(uri!),
        };
    }

    internal static byte[] StreamToByteArray(this Stream stream)
    {
        if (stream is MemoryStream memoryStream)
            return memoryStream.ToArray();

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
