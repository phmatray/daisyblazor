using Microsoft.Extensions.DependencyInjection;

namespace DaisyBlazor;

/// <summary>Extension methods that wire the DaisyBlazor component services into DI.</summary>
public static class DaisyBlazorServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core DaisyBlazor services: <see cref="ISnackbar"/> and
    /// <see cref="IDialogService"/> (both scoped). Call this from <c>Program.cs</c>.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDaisyBlazor(this IServiceCollection services) => services.AddDaisyBlazor(_ => { });

    /// <summary>
    /// Registers the core DaisyBlazor services (see <see cref="AddDaisyBlazor(IServiceCollection)"/>) and
    /// the app-wide component defaults, e.g. <c>o =&gt; o.DefaultButtonVariant = Variant.Filled</c>.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">Sets the defaults.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDaisyBlazor(this IServiceCollection services, Action<DaisyBlazorOptions> configure)
    {
        DaisyBlazorOptions options = new();
        configure(options);
        services.AddSingleton(options);
        services.AddScoped<ISnackbar, SnackbarService>();
        services.AddScoped<IDialogService, DialogService>();
        return services;
    }

    /// <summary>
    /// Registers the daisyUI-returning breadcrumb service, localised via
    /// <c>IStringLocalizer&lt;TResource&gt;</c>. The app must also register an
    /// <see cref="IBreadcrumbSource"/> (see the overload that registers one for you).
    /// </summary>
    /// <typeparam name="TResource">The resource type used to resolve localized breadcrumb titles.</typeparam>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">Optional callback to customise the options (home icon, home title key).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDaisyBlazorBreadcrumbs<TResource>(
        this IServiceCollection services,
        Action<BreadcrumbServiceOptions>? configure = null)
        where TResource : class
    {
        BreadcrumbServiceOptions options = new BreadcrumbServiceOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddScoped<IBreadcrumbService, BreadcrumbService<TResource>>();
        return services;
    }

    /// <summary>
    /// Registers the breadcrumb service together with the app's <see cref="IBreadcrumbSource"/>
    /// implementation (scoped).
    /// </summary>
    /// <typeparam name="TResource">The resource type used to resolve localized breadcrumb titles.</typeparam>
    /// <typeparam name="TSource">The app's <see cref="IBreadcrumbSource"/> implementation.</typeparam>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">Optional callback to customise the options (home icon, home title key).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDaisyBlazorBreadcrumbs<TResource, TSource>(
        this IServiceCollection services,
        Action<BreadcrumbServiceOptions>? configure = null)
        where TResource : class
        where TSource : class, IBreadcrumbSource
    {
        services.AddScoped<IBreadcrumbSource, TSource>();
        return services.AddDaisyBlazorBreadcrumbs<TResource>(configure);
    }
}
