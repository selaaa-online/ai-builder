using System.Reflection;
using AIBuilder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Tool/function-calling registration extensions for <see cref="IAIBuilder"/>.
/// </summary>
public static class AIBuilderToolsExtensions
{
    /// <summary>
    /// Enables automatic tool invocation in the pipeline. Required for registered tools to be executed.
    /// </summary>
    public static IAIBuilder UseTools(this IAIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.RequireChatClientBuilder().UseFunctionInvocation();
        return builder;
    }

    /// <summary>Registers a pre-built <see cref="AITool"/>.</summary>
    public static IAIBuilder AddTool(this IAIBuilder builder, AITool tool)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(tool);
        builder.Services.AddSingleton(tool);
        return builder;
    }

    /// <summary>Registers a delegate as a tool.</summary>
    /// <param name="builder">The AIBuilder instance.</param>
    /// <param name="method">The delegate to expose as a tool.</param>
    /// <param name="name">An optional tool name. Defaults to the method name.</param>
    /// <param name="description">An optional tool description.</param>
    public static IAIBuilder AddTool(this IAIBuilder builder, Delegate method, string? name = null, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(method);

        AIFunction function = name is null && description is null
            ? AIFunctionFactory.Create(method)
            : AIFunctionFactory.Create(method, name, description);

        builder.Services.AddSingleton<AITool>(function);
        return builder;
    }

    /// <summary>
    /// Registers all public instance methods of <typeparamref name="T"/> as tools.
    /// The type is resolved from the container so its dependencies are injected.
    /// </summary>
    public static IAIBuilder AddTools<T>(this IAIBuilder builder)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddSingleton<T>();

        MethodInfo[] methods = typeof(T).GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (MethodInfo method in methods)
        {
            if (method.IsSpecialName)
            {
                continue;
            }

            MethodInfo captured = method;
            builder.Services.AddSingleton<AITool>(sp =>
                AIFunctionFactory.Create(captured, sp.GetRequiredService<T>()));
        }

        return builder;
    }
}
