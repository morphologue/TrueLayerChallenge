using System;
using WireMock.Server;
using WireMock.Settings;

namespace Morphologue.Challenges.TrueLayer.Tests.Integration.TestHelpers;

public class WireMockServerFixture : IDisposable
{
    internal IWireMockServer WireMock { get; }

    public WireMockServerFixture()
    {
        WireMock = WireMockServer.Start(new WireMockServerSettings
        {
            Urls = new[] { "http://localhost:5010" },
            ReadStaticMappings = true,
            AllowPartialMapping = true,
            FileSystemHandler = new EmbeddedResourceFileSystemHandler()
        });
    }

    public void Dispose()
    {
        WireMock.Stop();
        WireMock.Dispose();
    }
}
