Task("Default")
    .IsDependentOn("Test-Coverage")
    .Does(() =>
{
    Information("Build com Cake iniciado!");
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
    var resultsDir  = "./coverage";
    var reportDir   = "./CoverageReport";
    var coverageXml = $"{resultsDir}/coverage.cobertura.xml";

    Information("Limpando pasta de cobertura...");
    CleanDirectory(resultsDir);

    var coverageToolExe    = "./tools/dotnet-coverage";
    var coverageToolExeWin = "./tools/dotnet-coverage.exe";

    if (!FileExists(coverageToolExe) && !FileExists(coverageToolExeWin))
    {
        Information("Instalando dotnet-coverage em ./tools...");
        var installCode = StartProcess("dotnet", "tool install --tool-path ./tools dotnet-coverage --ignore-failed-sources");

        if (installCode != 0)
        {
            throw new Exception($"Instalação do dotnet-coverage falhou ({installCode})");
        }
    }

    var coverageToolPath = FileExists(coverageToolExeWin) ? coverageToolExeWin : coverageToolExe;

    Information("Executando testes com cobertura (dotnet-coverage)...");

    // dotnet-coverage collect monitors child processes, which is essential for Aspire integration tests
    var testArgs = $"test \"{testProject}\" --configuration Release --no-build --logger \"trx;LogFileName=test-results.trx\"";
    var collectArgs = $"collect --output-format cobertura --output \"{coverageXml}\" \"dotnet {testArgs}\"";
    var exitCode = StartProcess(coverageToolPath, collectArgs);

    if (exitCode != 0)
    {
        throw new Exception($"dotnet-coverage falhou ({exitCode})");
    }

    var reports = GetFiles(coverageXml);

    if (reports == null || reports.Count == 0)
    {
        throw new Exception("Nenhum arquivo de cobertura encontrado após os testes.");
    }

    var reportGeneratorExe    = "./tools/reportgenerator";
    var reportGeneratorExeWin = "./tools/reportgenerator.exe";

    if (!FileExists(reportGeneratorExe) && !FileExists(reportGeneratorExeWin))
    {
        Information("Instalando ReportGenerator em ./tools...");
        var installCode = StartProcess("dotnet", "tool install --tool-path ./tools dotnet-reportgenerator-globaltool");

        if (installCode != 0)
        {
            throw new Exception($"Instalação do ReportGenerator falhou ({installCode})");
        }
    }

    var reportArgs = string.Join(";", reports.Select(f => f.FullPath));
    var rgPath     = FileExists(reportGeneratorExeWin) ? reportGeneratorExeWin : reportGeneratorExe;

    Information("Gerando relatório HTML...");
    var rgCode = StartProcess(rgPath, $"-reports:\"{reportArgs}\" -targetdir:\"{reportDir}\" -reporttypes:\"Html;Cobertura;MarkdownSummaryGithub\" -assemblyfilters:\"+MVFC.Aspire.Helpers*\" -classfilters:\"-*.Tests.*;-*.Playground.*\"");

    if (rgCode != 0)
    {
        throw new Exception($"ReportGenerator falhou ({rgCode})");
    }

    Information($"Relatório gerado em: {reportDir}");
});

RunTarget("Default");
