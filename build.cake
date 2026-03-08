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
    CleanDirectory("./coverage");
    CleanDirectory("./CoverageReport");
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Information("Restaurando pacotes...");
    StartProcess("dotnet", "restore MVFC.Aspire.Helpers.slnx");
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    Information("Build Release...");
    StartProcess("dotnet", "build MVFC.Aspire.Helpers.slnx --configuration Release --no-restore");
});

Task("Test-Coverage")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testProject = "./test/MVFC.Aspire.Helpers.Tests/MVFC.Aspire.Helpers.Tests.csproj";
    var resultsDir  = "./coverage";
    var reportDir   = "./CoverageReport";

    Information("Executando testes com cobertura...");
    StartProcess("dotnet", $"test \"{testProject}\" --configuration Release --no-build --collect:\"XPlat Code Coverage\" --results-directory \"{resultsDir}\" --settings coverage.runsettings --logger \"trx;LogFileName=test-results.trx\"");

    var reports = GetFiles("./coverage/**/coverage.cobertura.xml");
    if (reports == null || reports.Count == 0)
    {
        Warning("Nenhum arquivo de cobertura encontrado.");
        return;
    }

    var reportGeneratorExe    = "./tools/reportgenerator";
    var reportGeneratorExeWin = "./tools/reportgenerator.exe";
    if (!FileExists(reportGeneratorExe) && !FileExists(reportGeneratorExeWin))
    {
        Information("Instalando ReportGenerator em ./tools...");
        StartProcess("dotnet", "tool install --tool-path ./tools dotnet-reportgenerator-globaltool");
    }

    var reportArgs = string.Join(";", reports.Select(f => f.FullPath));
    var rgPath     = FileExists(reportGeneratorExeWin) ? reportGeneratorExeWin : reportGeneratorExe;

    Information("Gerando relatório HTML...");
    StartProcess(rgPath, $"-reports:\"{reportArgs}\" -targetdir:\"{reportDir}\" -reporttypes:\"Html;Cobertura;MarkdownSummaryGithub\" -assemblyfilters:\"+MVFC.Aspire.Helpers.*\" -classfilters:\"-*.Tests.*;-*.Playground.*\"");
    Information($"Relatório gerado em: {reportDir}");
});

RunTarget("Default");
