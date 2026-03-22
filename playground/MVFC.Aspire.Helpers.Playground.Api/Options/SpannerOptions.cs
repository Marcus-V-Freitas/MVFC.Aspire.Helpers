namespace MVFC.Aspire.Helpers.Playground.Api.Options;

public sealed record SpannerOptions(
    string ProjectId,
    string InstanceId,
    string DatabaseId);
