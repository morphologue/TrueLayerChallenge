namespace Morphologue.Challenges.TrueLayer
{
    /// <summary>Tag a class as being suitable for registration in a DI container. The class will be registered against
    /// its interface(s) and will have a singleton lifetime.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonServiceAttribute : Attribute { }
}
