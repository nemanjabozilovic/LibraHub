namespace LibraHub.Gateway.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGatewaySwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "LibraHub Gateway API v1");
            c.RoutePrefix = string.Empty;
        });

        return app;
    }
}

