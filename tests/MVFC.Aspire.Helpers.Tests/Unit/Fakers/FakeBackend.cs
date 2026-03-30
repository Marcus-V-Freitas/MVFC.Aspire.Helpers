namespace MVFC.Aspire.Helpers.Tests.Unit.Fakers;

public sealed class FakeBackend(string name)
    : Resource(name), IResourceWithEndpoints, IResourceWithEnvironment
{
}
