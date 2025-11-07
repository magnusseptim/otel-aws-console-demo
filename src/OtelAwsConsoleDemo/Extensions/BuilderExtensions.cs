
namespace OtelAwsConsoleDemo.Extensions;

public static class BuilderExtensions
{
    public static TBuilder AddIf<TBuilder>(this TBuilder builder, bool cond, Action<TBuilder> add)
    {
        if (cond) add(builder);
        return builder;
    }
}
