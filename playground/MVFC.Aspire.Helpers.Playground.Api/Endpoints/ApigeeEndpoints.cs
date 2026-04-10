namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class ApigeeEndpoints
{
    private static readonly string[] _endpoints =
        [ "/", "/health", "/echo", "/transform", "/quota-test",
          "/info", "/cached", "/admin", "/secure", "/xml" ];

    private static readonly string[] _permissions = ["read", "write", "admin"];
    private static readonly string[] _headers =
    [
        "X-Correlation-Id",
        "X-Request-Id",
        "X-Apigee-Env",
        "X-Apigee-Proxy",
        "X-Logging-Timestamp",
    ];

    public static void MapApigeeEndpoints(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapRootEndpoint();
        apiGroup.MapHealthEndpoint();
        apiGroup.MapEchoEndpoint();
        apiGroup.MapTransformEndpoint();
        apiGroup.MapQuotaTestEndpoint();
        apiGroup.MapInfoEndpoint();
        apiGroup.MapCachedEndpoint();
        apiGroup.MapAdminEndpoint();
        apiGroup.MapSecureEndpoint();
        apiGroup.MapXmlEndpoint();
        apiGroup.MapHealthCheckEndpoint();
        apiGroup.MapSharedFlowCheckEndpoint();
    }

    private static void MapRootEndpoint(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/", () => new
        {
            message = "Hello from BackendApi (Aspire)",
            timestamp = DateTimeOffset.UtcNow.DateTime,
        });
    }

    private static void MapHealthEndpoint(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/health", () => Results.Ok("healthy"));
    }

    private static void MapEchoEndpoint(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/echo", (HttpRequest req) => Results.Ok(new
        {
            method = req.Method,
            path = req.Path.Value,
            headers = req.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
        }));
    }

    private static void MapTransformEndpoint(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/transform", () => Results.Ok(new
        {
            products = new[]
            {
                new { id = 1, name = "Widget A", price = 29.99, inStock = true },
                new { id = 2, name = "Widget B", price = 49.99, inStock = false },
                new { id = 3, name = "Widget C", price = 19.99, inStock = true },
            },
            total = 3,
            currency = "BRL"
        }));
    }

    private static void MapQuotaTestEndpoint(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/quota-test", () => Results.Ok(new
        {
            message = "Quota test endpoint",
            callTime = DateTimeOffset.UtcNow.DateTime,
            note = "Este endpoint tem limite de 5 chamadas por minuto via Apigee Quota policy"
        }));
    }

    private static void MapInfoEndpoint(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/info", () => Results.Ok(new
        {
            server = "BackendApi",
            framework = "ASP.NET Minimal API",
            runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            os = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            timestamp = DateTimeOffset.UtcNow.DateTime,
            endpoints = _endpoints,
        }));
    }

    private static void MapCachedEndpoint(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/cached", async () =>
        {
            await Task.Delay(2000).ConfigureAwait(false);
            return Results.Ok(new
            {
                message = "Dados processados (operação custosa)",
                generatedAt = DateTimeOffset.UtcNow.DateTime,
                data = new
                {
                    metrics = new
                    {
                        cpu = 42.5,
                        memory = 67.3,
                        disk = 23.1,
                    },
                    users = 1234,
                    requestsPerHour = 56789
                },
                note = "Esta resposta deve ser cacheada por 30s pelo proxy Apigee",
            });
        });
    }

    private static void MapAdminEndpoint(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/admin", () => Results.Ok(new
        {
            panel = "Admin Dashboard",
            status = "active",
            uptime = TimeSpan.FromMinutes(Random.Shared.Next(100, 9999)).ToString(),
            connectedUsers = Random.Shared.Next(1, 50),
            systemHealth = "green",
            note = "Este endpoint é restrito a IPs locais via AccessControl policy",
        }));
    }

    private static void MapSecureEndpoint(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/secure", (HttpRequest req) =>
        {
            var authUser = req.Headers["X-Authenticated-User"].FirstOrDefault() ?? "unknown";
            return Results.Ok(new
            {
                message = "Acesso autorizado ao recurso protegido!",
                authenticatedUser = authUser,
                secretData = new
                {
                    apiKey = "sk-demo-" + Guid.NewGuid().ToString("N")[..16],
                    environment = "development",
                    permissions = _permissions,
                },
                note = "Este endpoint requer Basic Auth (user: admin, pw: secret123)"
            });
        });
    }

    private static void MapXmlEndpoint(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/xml", () =>
        {
            var xml = """
                       <?xml version="1.0" encoding="UTF-8"?>
                       <catalog>
                         <book id="1">
                           <title>API Design Patterns</title>
                           <author>JJ Geewax</author>
                           <year>2021</year>
                           <price>49.99</price>
                           <available>true</available>
                         </book>
                         <book id="2">
                           <title>Designing Web APIs</title>
                           <author>Brenda Jin</author>
                           <year>2018</year>
                           <price>39.99</price>
                           <available>true</available>
                         </book>
                         <book id="3">
                           <title>RESTful Web Services</title>
                           <author>Leonard Richardson</author>
                           <year>2007</year>
                           <price>29.99</price>
                           <available>false</available>
                         </book>
                       </catalog>
                       """;
            return Results.Content(xml, "application/xml");
        });
    }

    private static void MapHealthCheckEndpoint(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/health-check", () => Results.Ok(new
        {
            status = "healthy",
            checks = new
            {
                database = "connected",
                cache = "available",
                externalApis = "reachable",
            },
            version = "1.0.0",
            timestamp = DateTimeOffset.UtcNow.DateTime,
        }));
    }

    private static void MapSharedFlowCheckEndpoint(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/sharedflow-check", (HttpRequest request) =>
        {
            var headersPresentes = _headers
                .Select(h => new
                {
                    Header = h,
                    Valor = request.Headers.TryGetValue(h, out var v) ? v.ToString() : null,
                    Presente = request.Headers.ContainsKey(h)
                })
                .ToList();

            var sharedFlowAtivo = headersPresentes.Any(h => h.Presente);

            return Results.Ok(new
            {
                SharedFlowDetectado = sharedFlowAtivo,
            });
        })
        .WithName("SharedFlowCheck")
        .WithTags("SharedFlow");
    }
}
