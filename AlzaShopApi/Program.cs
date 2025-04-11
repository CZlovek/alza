using System.Reflection;
using AlzaShopApi.Database;
using AlzaShopApi.Services;
using AlzaShopApi.Services.Interfaces;
using AlzaShopApi.Toolkit.Brokers;
using AlzaShopApi.Toolkit.Brokers.Interfaces;
using AlzaShopApi.Toolkit.Swagger;
using Asp.Versioning;
using Czlovek.Logger;
using Czlovek.RabbitMq.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AlzaShopApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddCustomLogger(builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddRabbitMq(builder.Configuration);

        builder.Services.AddHostedService<RabbitMqUpdateService>();

        AddApiVersioning(builder);
        AddSwagger(builder);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));

            options.EnableSensitiveDataLogging(true);
        });

        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddSingleton<IMessageBroker, MessageBroker>();

        var application = builder.Build();

        UseSwagger(application);

        UseDatabase(application);

        application.UseHttpsRedirection();

        application.UseCors(x => x
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed(origin => true));

        application.UseAuthentication();
        application.UseAuthorization();

        application.MapControllers();

        application.Run();
    }

    private static void UseSwagger(WebApplication application)
    {
        if (application.Environment.IsDevelopment())
        {
            application.UseSwagger();

            application.UseSwaggerUI(options =>
            {
                options.DocumentTitle = "AlzaShop Api Dokumentace";

                foreach (var description in application.DescribeApiVersions())
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                        $"AlzaShopApi verze {description.ApiVersion.ToString()}");
                }
            });
        }
    }

    private static void AddApiVersioning(WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = false;
            //options.ApiVersionReader = ApiVersionReader.Combine(
            // new QueryStringApiVersionReader("v")
            // new HeaderApiVersionReader("x-api-version")
            // new MediaTypeApiVersionReader("apiVersion")
            //);
        }).AddApiExplorer(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }

    private static void AddSwagger(WebApplicationBuilder builder)
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

        builder.Services.AddSwaggerGen(options =>
        {
            options.DocumentFilter<LowercaseDocumentFilter>();
            options.IncludeXmlComments(xmlPath, true);
        });

        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    }

    private static void UseDatabase(WebApplication application)
    {
        using var scope = application.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Database.EnsureCreated();
        context.SeedData();
    }
}