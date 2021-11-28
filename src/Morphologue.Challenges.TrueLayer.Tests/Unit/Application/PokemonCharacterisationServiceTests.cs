using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Morphologue.Challenges.TrueLayer.Application;
using Morphologue.Challenges.TrueLayer.Interfaces.Application;
using Morphologue.Challenges.TrueLayer.Interfaces.Infrastructure;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Morphologue.Challenges.TrueLayer.Tests.Unit.Application;

public class PokemonCharacterisationServiceTests
{
    private readonly Mock<ITranslationRequestCommandFactory> _mockTranslationRequestCommandFactory;
    private readonly IPokemonCharacterisationService _patient;

    private PokemonSpeciesResponse _mappedSpeciesResponse = new(
        IsLegendary: false,
        LocalisedDescriptions: new[] { new LocalisedDescription("en", "some weird thing") },
        HabitatName: "urban");
    private string? _translatedDescription = "translation";

    public PokemonCharacterisationServiceTests()
    {
        var mockSpeciesCommand = new Mock<IRequestCommand<PokemonSpeciesResponse>>();
        mockSpeciesCommand.Setup(m => m.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _mappedSpeciesResponse ?? throw new InvalidOperationException());

        var mockPokemonRequestCommandFactory = new Mock<IPokemonRequestCommandFactory>();
        mockPokemonRequestCommandFactory.Setup(m => m.CreatePokemonRequestCommand(It.IsAny<string>()))
            .Returns<string>(name =>
            {
                var mockPokemonCommand = new Mock<IRequestCommand<PokemonResponse>>();
                mockPokemonCommand.Setup(m => m.ExecuteAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new PokemonResponse(name, mockSpeciesCommand.Object));
                return mockPokemonCommand.Object;
            });

        var mockTranslationCommand = new Mock<IRequestCommand<string>>();
        mockTranslationCommand.Setup(m => m.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _translatedDescription ?? throw new Exception("testing"));

        _mockTranslationRequestCommandFactory = new Mock<ITranslationRequestCommandFactory>();
        _mockTranslationRequestCommandFactory.Setup(m => m.CreateTranslationCommand(It.IsAny<string>(), It.IsAny<TranslationTarget>()))
            .Returns(mockTranslationCommand.Object);

        _patient = new PokemonCharacterisationService(
            mockPokemonRequestCommandFactory.Object,
            _mockTranslationRequestCommandFactory.Object,
            new Mock<ILogger<PokemonCharacterisationService>>().Object);
    }

    [Fact]
    public async Task CharacteriseAsync_ThrowsNotSupportedException_WhenNoEnglishDescriptionIsAvailable()
    {
        _mappedSpeciesResponse = _mappedSpeciesResponse with { LocalisedDescriptions = Enumerable.Empty<LocalisedDescription>() };

        var action = () => _patient.CharacteriseAsync("pokewot", translate: true, default);

        (await action.Should().ThrowAsync<NotSupportedException>())
            .Which.Message.Should().Contain("English");
    }

    [Fact]
    public async Task CharacteriseAsync_UsesEnglishDescription_WhenTranslationThrowsException()
    {
        _translatedDescription = null;

        var result = await _patient.CharacteriseAsync("pokewot", translate: true, default);

        result.Description.Should().Be("some weird thing");
    }

    [Theory]
    [InlineData("plateau", true, TranslationTarget.Yoda)]
    [InlineData("cave", false, TranslationTarget.Yoda)]
    [InlineData("cave", true, TranslationTarget.Yoda)]
    [InlineData("plateau", false, TranslationTarget.IncorrectEarlyModernEnglish)]
    [InlineData(null, false, TranslationTarget.IncorrectEarlyModernEnglish)]
    public async Task CharacteriseAsync_TargetsYodaTranslation_OnlyGivenCaveHabitatOrLegendary(
        string? habitatName,
        bool isLegendary,
        TranslationTarget expected)
    {
        _mappedSpeciesResponse = _mappedSpeciesResponse with
        {
            HabitatName = habitatName,
            IsLegendary = isLegendary
        };

        await _patient.CharacteriseAsync("pokewot", translate: true, default);

        _mockTranslationRequestCommandFactory.Verify(m => m.CreateTranslationCommand(
                "some weird thing",
                expected),
            Times.Once);
    }

    [Theory]
    [InlineData(false, "some weird thing")]
    [InlineData(true, "translation")]
    public async Task CharacteriseAsync_AppliesTranslation_OnlyWhenRequested(bool translated, string expected)
    {
        var result = await _patient.CharacteriseAsync("pokewot", translated, default);

        result.Description.Should().Be(expected);
    }
}
