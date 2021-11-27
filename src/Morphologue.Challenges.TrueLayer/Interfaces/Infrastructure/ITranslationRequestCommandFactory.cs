namespace Morphologue.Challenges.TrueLayer.Interfaces.Infrastructure
{
    public interface ITranslationRequestCommandFactory
    {
        IRequestCommand<string> CreateTranslationCommand(string original, TranslationTarget target);
    }

    public enum TranslationTarget
    {
        IncorrectEarlyModernEnglish,
        Yoda
    }
}
