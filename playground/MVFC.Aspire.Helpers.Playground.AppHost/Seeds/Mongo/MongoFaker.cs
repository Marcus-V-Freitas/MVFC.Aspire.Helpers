namespace MVFC.Aspire.Helpers.Playground.AppHost.Seeds.Mongo;

internal static class MongoFaker
{
    internal static Faker<TestDatabase> GenerateFaker() =>
        new Faker<TestDatabase>()
            .CustomInstantiator(f => new TestDatabase(
                Name: f.Person.FullName,
                Cnpj: f.Company.Cnpj()))
            .RuleFor(x => x.Name, f => f.Person.FullName)
            .RuleFor(x => x.Cnpj, f => f.Company.Cnpj());
}
