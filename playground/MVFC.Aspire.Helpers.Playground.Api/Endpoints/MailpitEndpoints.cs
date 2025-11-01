namespace MVFC.Aspire.Helpers.Playground.Api.Endpoints;

public static class MailpitEndpoints {

    public static void MapMailpitEndpoints(this IEndpointRouteBuilder apiGroup) =>
        apiGroup.MapPost("/send-email", async (SmtpClient client, SmtpRequest request) => {
            using var message = new MailMessage(request.From, request.To) {
                Subject = request.Subject,
                Body = request.Body
            };

            await client.SendMailAsync(message);
            return Results.Ok("Email enviado com sucesso!");
        });
}