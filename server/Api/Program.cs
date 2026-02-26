

using Api;
using Api.Services;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

Configure(app);

await app.RunAsync();






public static partial class Program
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // todo connectionstring
        
        // todo AddIdentity
        
        services.AddScoped<IEventService, EventService>();
        
        // todo JWT bearer + key + settings

        services.AddControllers();
        services.AddOpenApiDocument();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // todo CORS

    }

    public static void Configure(WebApplication app)
    {
        //use exceptionhandler
        app.UseRouting();
        //cors
        //use auth and authz
        app.UseExceptionHandler();

        app.MapControllers();
    }
}