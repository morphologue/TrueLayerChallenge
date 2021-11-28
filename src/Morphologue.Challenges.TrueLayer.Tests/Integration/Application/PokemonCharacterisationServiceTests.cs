using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Morphologue.Challenges.TrueLayer.Application;
using Morphologue.Challenges.TrueLayer.Interfaces.Application;
using Morphologue.Challenges.TrueLayer.Tests.Integration.TestHelpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using WireMock.Server;
using Xunit;

namespace Morphologue.Challenges.TrueLayer.Tests.Integration.Application;

public class PokemonCharacterisationServiceTests : IClassFixture<WireMockServerFixture>
{
    private readonly IPokemonCharacterisationService _patient;
    private readonly IWireMockServer _wireMock;

    public PokemonCharacterisationServiceTests(WireMockServerFixture wireMockFixture)
    {
        _wireMock = wireMockFixture.WireMock;

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(m => m["PokeAPIPokemonUrlPrefix"])
            .Returns(() => _wireMock.Urls[0] + "/api/v2/pokemon");
        mockConfiguration.Setup(m => m["FunTranslationsUrlPrefix"])
            .Returns(() => _wireMock.Urls[0] + "/translate");

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
            Habitat = "rare",
            IsLegendary = true
        } },
        new object[] { "alakazam", false, new {
            Name = "alakazam",
            Description = "Its brain can out­ perform a super­ computer. Its intelligence quotient is said to be 5,000.",
            Habitat = "urban",
            IsLegendary = false
        } },
        new object[] { "mewtwo", true, new {
            Name = "mewtwo",
            Description = "Created by a scientist after years of horrific gene splicing and dna engineering experiments, it was.",
            Habitat = "rare",
            IsLegendary = true
        } },
        new object[] { "alakazam", true, new {
            Name = "alakazam",
            Description = "Its brain can out­ perform a super­ computer. Its intelligence quotient is did doth sayeth to beest 5,000.",
            Habitat = "urban",
            IsLegendary = false
        } }
    };

    [Theory]
    [MemberData(nameof(HappyPathCases))]
    public async Task CharacteriseAsync_CharacterisesAndOptionallyTranslates_AsRequested(string name, bool translated, object expected)
    {
        var result = await _patient.CharacteriseAsync(name, translated, default);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task CharacteriseAsync_ThrowsNotFoundException_WhenPokemonNotFound()
    {
        var action = () => _patient.CharacteriseAsync("asdfh", true, default);

        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CharacteriseAsync_ProvidesUntranslatedDescription_WhenTranslatorIsRateLimited()
    {
        var result = await _patient.CharacteriseAsync("snorlax", true, default);

        result.Should().BeEquivalentTo(new
        {
            Name = "snorlax",
            Description = "Very lazy. Just eats and sleeps. As its rotund bulk builds, it becomes steadily more slothful.",
            Habitat = "mountain",
            IsLegendary = false
        });
    }
}
