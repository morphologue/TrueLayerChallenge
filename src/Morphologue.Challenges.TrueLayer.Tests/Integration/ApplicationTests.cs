using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Morphologue.Challenges.TrueLayer.Application;
using Morphologue.Challenges.TrueLayer.Interfaces.Application;
using Morphologue.Challenges.TrueLayer.Tests.Integration.TestHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WireMock.Server;
using WireMock.Settings;
using Xunit;

namespace Morphologue.Challenges.TrueLayer.Tests.Integration;

public class ApplicationTests : IDisposable
{
    private readonly IPokemonCharacterisationService _patient;
    private readonly IWireMockServerSettings _wireMockSettings;

    private IWireMockServer? _wireMock;

    public ApplicationTests()
    {
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(m => m["PokeAPIPokemonUrlPrefix"])
            .Returns(() => (_wireMock?.Urls[0] ?? throw new InvalidOperationException()) + "/api/v2/pokemon");
        mockConfiguration.Setup(m => m["FunTranslationsUrlPrefix"])
            .Returns(() => (_wireMock?.Urls[0] ?? throw new InvalidOperationException()) + "/translate");

        _wireMockSettings = new WireMockServerSettings
        {
            ReadStaticMappings = true,
            AllowPartialMapping = true
        };

        var services = new ServiceCollection();
        services.AddSingleton(mockConfiguration.Object);
        services.AddHttpClient();
        services.Scan(scan =>
            scan.FromAssemblyOf<PokemonCharacterisationService>()
                .AddClasses(classes => classes.WithAttribute<SingletonServiceAttribute>())
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime());

        _patient = services.BuildServiceProvider().GetRequiredService<IPokemonCharacterisationService>();
    }

    public static IEnumerable<object[]> HappyPathCases = new[]
    {
        new object[] { "mewtwo", false, new {
            Name = "mewtwo",
            Description = "It was created by a scientist after years of horrific gene splicing and DNA engineering experiments.",
            HabitatName = "rare",
            IsLegendary = true
        } },
        new object[] { "alakazam", false, new {
            Name = "alakazam",
            Description = "Its brain can out­ perform a super­ computer. Its intelligence quotient is said to be 5,000.",
            HabitatName = "urban",
            IsLegendary = false
        } },
        new object[] { "mewtwo", true, new {
            Name = "mewtwo",
            Description = "Created by a scientist after years of horrific gene splicing and dna engineering experiments, it was.",
            HabitatName = "rare",
            IsLegendary = true
        } },
        new object[] { "alakazam", true, new {
            Name = "alakazam",
            Description = "Its brain can out­ perform a super­ computer. Its intelligence quotient is did doth sayeth to beest 5,000.",
            HabitatName = "urban",
            IsLegendary = false
        } }
    };

    [Theory]
    [MemberData(nameof(HappyPathCases))]
    public async Task HappyPath(string name, bool translated, object expected)
    {
        StartWireMock("HappyMappings");

        var result = await _patient.CharacteriseAsync(name, translated, default);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task NotFoundException_WhenPokemonNotFound()
    {
        StartWireMock("NotFoundMappings");

        var action = () => _patient.CharacteriseAsync("asdfh", true, default);

        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UntranslatedDescription_WhenTranslatorFails()
    {
        StartWireMock("RateLimitedMappings");

        var result = await _patient.CharacteriseAsync("alakazam", true, default);

        result.Should().BeEquivalentTo(new
        {
            Name = "alakazam",
            Description = "Its brain can out­ perform a super­ computer. Its intelligence quotient is said to be 5,000.",
            HabitatName = "urban",
            IsLegendary = false
        });
    }

    #region Helpers
    public void Dispose()
    {
        if (_wireMock == null)
        {
            return;
        }
        _wireMock.Stop();
        _wireMock.Dispose();
    }

    private void StartWireMock(string resourceSubdirectory)
    {
        _wireMockSettings.FileSystemHandler = new EmbeddedResourceFileSystemHandler(resourceSubdirectory);
        _wireMock = WireMockServer.Start(_wireMockSettings);
    }
    #endregion
}
