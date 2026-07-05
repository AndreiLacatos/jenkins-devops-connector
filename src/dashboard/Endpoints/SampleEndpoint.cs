namespace dashboard.Endpoints;

internal static class SampleEndpoint
{
    internal static IEndpointRouteBuilder MapSampleEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/sample", HandleSample);
        return builder;
    }

    private static async Task<IResult> HandleSample()
    {
        return Results.Ok(new
        {
            data = new List<dynamic>
            {
                new { foo = "bar" },
                new { foo = "baz" },
            },
        });
    }
}
