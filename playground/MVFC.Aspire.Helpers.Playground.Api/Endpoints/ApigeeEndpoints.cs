namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class ApigeeEndpoints
{
    public static void MapApigeeEndpoints(this IEndpointRouteBuilder apiGroup)
    {
        apiGroup.MapGet("/", () => new
        {
            message = "Hello from BackendApi (Aspire)",
            timestamp = DateTime.UtcNow
        });

        apiGroup.MapGet("/health", () => Results.Ok("healthy"));

        apiGroup.MapGet("/echo", (HttpRequest req) => Results.Ok(new
        {
            method = req.Method,
            path = req.Path.Value,
            headers = req.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
        }));

        // ─── Endpoints Fase 1 ───────────────────────────────────────────────────────

        // Transform — dados ricos para demonstrar o JavaScript transform envelope
        apiGroup.MapGet("/transform", () => Results.Ok(new
        {
            products = new[]
            {
        new { id = 1, name = "Widget A", price = 29.99, inStock = true },
        new { id = 2, name = "Widget B", price = 49.99, inStock = false },
        new { id = 3, name = "Widget C", price = 19.99, inStock = true }
    },
            total = 3,
            currency = "BRL"
        }));

        // Quota test — endpoint simples para testar rate limiting
        apiGroup.MapGet("/quota-test", () => Results.Ok(new
        {
            message = "Quota test endpoint",
            callTime = DateTime.UtcNow,
            note = "Este endpoint tem limite de 5 chamadas por minuto via Apigee Quota policy"
        }));

        // Info — metadata do servidor
        apiGroup.MapGet("/info", () => Results.Ok(new
        {
            server = "BackendApi",
            framework = "ASP.NET Minimal API",
            runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            os = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            timestamp = DateTime.UtcNow,
            endpoints = new[] { "/", "/health", "/echo", "/transform", "/quota-test", "/info",
                         "/cached", "/admin", "/secure", "/xml" }
        }));

        // ─── Endpoints Fase 2 ───────────────────────────────────────────────────────

        // Cached — resposta com delay simulado para demonstrar cache
        apiGroup.MapGet("/cached", async () =>
        {
            // Simula processamento pesado (2 segundos)
            await Task.Delay(2000);
            return Results.Ok(new
            {
                message = "Dados processados (operação custosa)",
                generatedAt = DateTime.UtcNow,
                data = new
                {
                    metrics = new { cpu = 42.5, memory = 67.3, disk = 23.1 },
                    users = 1234,
                    requestsPerHour = 56789
                },
                note = "Esta resposta deve ser cacheada por 30s pelo proxy Apigee"
            });
        });

        // Admin — painel admin (protegido por AccessControl IP)
        apiGroup.MapGet("/admin", () => Results.Ok(new
        {
            panel = "Admin Dashboard",
            status = "active",
            uptime = TimeSpan.FromMinutes(Random.Shared.Next(100, 9999)).ToString(),
            connectedUsers = Random.Shared.Next(1, 50),
            systemHealth = "green",
            note = "Este endpoint é restrito a IPs locais via AccessControl policy"
        }));

        // Secure — recurso protegido (requer Basic Auth)
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
                    permissions = new[] { "read", "write", "admin" }
                },
                note = "Este endpoint requer Basic Auth (user: admin, pw: secret123)"
            });
        });

        // XML — resposta em formato XML para demonstrar XMLToJSON
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

        // Health check detalhado
        apiGroup.MapGet("/health-check", () => Results.Ok(new
        {
            status = "healthy",
            checks = new
            {
                database = "connected",
                cache = "available",
                externalApis = "reachable"
            },
            version = "1.0.0",
            timestamp = DateTime.UtcNow
        }));

    }
}
