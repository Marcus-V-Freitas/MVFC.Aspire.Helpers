namespace MVFC.Aspire.Helpers.Tests.Unit.ApigeeEmulator.Helpers;

public static class ApigeeEmulatorTestHelpers
{
    public static IResourceBuilder<ApigeeEmulatorResource> CreateBuilder()
    {
        var resource = new ApigeeEmulatorResource("apigee-test");
        var builder  = Substitute.For<IResourceBuilder<ApigeeEmulatorResource>>();
        builder.Resource.Returns(resource);

        return builder;
    }

    public static IResourceBuilder<FakeBackend> CreateBackend()
    {
        var resource = new FakeBackend("backend");
        var builder  = Substitute.For<IResourceBuilder<FakeBackend>>();
        builder.Resource.Returns(resource);

        return builder;
    }
}
