namespace MVFC.Aspire.Helpers.Playground.Api.Requests;

public sealed record SmtpRequest(
    string From,
    string To,
    string Subject,
    string Body);