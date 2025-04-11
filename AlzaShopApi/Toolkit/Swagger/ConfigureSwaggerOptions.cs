using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AlzaShopApi.Toolkit.Swagger;

/// <summary>
/// Configures Swagger documentation options for API versioning.
/// </summary>
/// <remarks>
/// This class implements <see cref="IConfigureNamedOptions{TOptions}"/> to provide configuration for Swagger generation options.
/// It uses the API version information from <see cref="IApiVersionDescriptionProvider"/> to create separate Swagger documents for each API version.
/// </remarks>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureNamedOptions<SwaggerGenOptions>
{
    /// <summary>
    /// Configures the Swagger generation options.
    /// </summary>
    /// <param name="options">The options instance to configure.</param>
    /// <remarks>
    /// This method applies the configuration to the provided options instance,
    /// creating separate Swagger documents for each API version in the application.
    /// </remarks>
    public void Configure(SwaggerGenOptions options)
    {
        ConfigureSwaggerDoc(options);
    }

    /// <summary>
    /// Configures the Swagger generation options with a specific name.
    /// </summary>
    /// <param name="name">The name of the options instance to configure.</param>
    /// <param name="options">The options instance to configure.</param>
    /// <remarks>
    /// This overload ignores the name parameter and delegates to the standard Configure method,
    /// ensuring consistent configuration regardless of how the options are accessed.
    /// </remarks>
    public void Configure(string? name, SwaggerGenOptions options)
    {
        ConfigureSwaggerDoc(options);
    }

    /// <summary>
    /// Configures the SwaggerDoc for each API version.
    /// </summary>
    /// <param name="options">The Swagger generation options to configure.</param>
    /// <remarks>
    /// This method iterates through all API version descriptions provided by the
    /// <see cref="IApiVersionDescriptionProvider"/> and creates a separate SwaggerDoc for each version.
    /// </remarks>
    private void ConfigureSwaggerDoc(SwaggerGenOptions options)
    {
        foreach (var versionDescription in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(versionDescription.GroupName, CreateVersion(versionDescription));
        }
    }

    /// <summary>
    /// Creates OpenAPI information for a specific API version.
    /// </summary>
    /// <param name="description">The API version description.</param>
    /// <returns>An <see cref="OpenApiInfo"/> object containing metadata for the API version.</returns>
    /// <remarks>
    /// This method builds the OpenAPI information object that appears in the Swagger UI,
    /// including title, version number, and description for a specific API version.
    /// </remarks>
    private OpenApiInfo CreateVersion(ApiVersionDescription description)
    {
        return new OpenApiInfo
        {
            Title = $"AlzaShop Api v{description.ApiVersion.ToString()}",
            Version = description.ApiVersion.ToString(),
            Description = "This document offers a basic overview of the API, outlining its core functionality and primary endpoints. For further technical details and advanced features, please refer to the full API documentation."
        };
    }
}
