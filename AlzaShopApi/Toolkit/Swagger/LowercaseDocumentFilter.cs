using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AlzaShopApi.Toolkit.Swagger;

/// <summary>
/// A custom Swagger document filter that converts all route paths in the Swagger
/// documentation to lowercase.
/// </summary>
/// <remarks>
/// This filter is applied to ensure consistent casing in API paths within the generated
/// Swagger document by converting all path keys to lowercase. This is particularly useful
/// in environments where the API paths are case-sensitive.
/// </remarks>
public class LowercaseDocumentFilter : IDocumentFilter
{
    /// <summary>
    /// Transforms all paths in the Swagger document to lowercase. This ensures uniformity for path keys in the generated Swagger documentation.
    /// </summary>
    /// <param name="swaggerDoc">The OpenAPI document to be modified.</param>
    /// <param name="context">The document filter context, providing additional information about the current operation.</param>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = swaggerDoc.Paths.ToDictionary(
            path => path.Key.ToLowerInvariant(),
            path => path.Value
        );

        swaggerDoc.Paths = new OpenApiPaths();

        foreach (var path in paths)
        {
            swaggerDoc.Paths.Add(path.Key, path.Value);
        }
    }
}