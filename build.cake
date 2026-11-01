Task("Default")
    .IsDependentOn("Test-Coverage")
    .Does(() =>
{
    Information("Build com Cake concluído!");
});

Task("Clean")
    .Does(() =>
{
    Information("Limpando pastas de resultados e relatórios...");
    CleanDirectory("./TestResults");
    CleanDirectory("./coverage");
    CleanDirectory("./CoverageReport");
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Information("Restaurando pacotes...");
    var exitCode = StartProcess("dotnet", "restore MVFC.Aspire.Helpers.slnx");
    if (exitCode != 0)
    {
        throw new Exception($"dotnet restore falhou ({exitCode})");
    }
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    Information("Build Release...");
    var exitCode = StartProcess("dotnet", "build MVFC.Aspire.Helpers.slnx --configuration Release --no-restore");
    if (exitCode != 0)
    {
        throw new Exception($"dotnet build falhou ({exitCode})");
    }
});

Task("Test-Coverage")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testProject = "./tests/MVFC.Aspire.Helpers.Tests/MVFC.Aspire.Helpers.Tests.csproj";
    var resultsDir = "./coverage";
    var reportDir = "./CoverageReport";
    var coverageXml = $"{resultsDir}/coverage.cobertura.xml";

    Information("Limpando pasta de cobertura...");
    CleanDirectory(resultsDir);

    // Ferramentas locais restauradas a partir de .config/dotnet-tools.json.
    // Executar 'dotnet tool restore' antes de rodar este Cake.
    Information("Executando testes com cobertura (dotnet-coverage)...");
    var testArgs = $"test \"{testProject}\" --configuration Release --no-build";
    var collectArgs = $"collect --output-format cobertura --output \"{coverageXml}\" \"dotnet {testArgs}\"";
    var exitCode = StartProcess("dotnet", $"tool run dotnet-coverage -- {collectArgs}");
    if (exitCode != 0)
    {
        throw new Exception($"dotnet-coverage falhou ({exitCode})");
    }

    var reports = GetFiles(coverageXml);
    if (reports == null || reports.Count == 0)
    {
        throw new Exception("Nenhum arquivo de cobertura encontrado após os testes.");
    }

    var reportArgs = string.Join(";", reports.Select(f => f.FullPath));
    Information("Gerando relatório HTML...");
    var rgArgs = $"-reports:\"{reportArgs}\" -targetdir:\"{reportDir}\" "
               + "-reporttypes:\"Html;Cobertura;MarkdownSummaryGithub\" "
               + "-assemblyfilters:\"+MVFC.Aspire.Helpers*\" "
               + "-classfilters:\"-*.Tests.*;-*.Playground.*\"";
    var rgCode = StartProcess("dotnet", $"tool run reportgenerator -- {rgArgs}");
    if (rgCode != 0)
    {
        throw new Exception($"ReportGenerator falhou ({rgCode})");
    }

    Information($"Relatório gerado em: {reportDir}");
});

RunTarget("Default");
