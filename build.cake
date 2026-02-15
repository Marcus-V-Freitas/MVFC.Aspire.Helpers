Task("Default")
    .IsDependentOn("Test-Coverage")
    .Does(() =>
{
    Information("Build com Cake iniciado!");
});

// Tasks adicionadas: Restore, Build, Test-Coverage
Task("Clean")
    .Does(() =>
{
    Information("Limpando pastas de resultados e relatorios...");
    CleanDirectory("./TestResults");
    CleanDirectory("./CoverageReport");
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Information("Restaurando pacotes dotnet...");
    StartProcess("dotnet", "restore");
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    Information("Build do solution (Release)...");
    StartProcess("dotnet", "build --configuration Release");
});

Task("Test-Coverage")
    .IsDependentOn("Build")
    .Does(() =>
{
    var solution = "./MVFC.Aspire.Helpers.slnx";
    var resultsDir = "./TestResults";
    var reportDir = "./CoverageReport";

    Information("Executando testes e coletando cobertura (coverlet.collector)...");
    // Executa dotnet test na solution inteira, coletando cobertura via coverlet.collector (XPlat Code Coverage)
    StartProcess("dotnet", $"test \"{solution}\" --configuration Release --no-build --collect:\"XPlat Code Coverage\" --results-directory \"{resultsDir}\"");

    // Procura arquivos gerados pelo coverlet.collector (coverage.cobertura.xml)
    var reports = GetFiles("./TestResults/**/coverage.cobertura.xml");
    if (reports == null || reports.Count == 0)
    {
        Warning("Nenhum arquivo de cobertura encontrado em './TestResults'. Verifique se os projetos de teste têm o pacote 'coverlet.collector' instalado e se os testes rodaram com sucesso.");
        return;
    }

    // Instala o ReportGenerator como ferramenta local em ./tools se ainda não existir
    var reportGeneratorExe = "./tools/reportgenerator";
    var reportGeneratorExeWin = "./tools/reportgenerator.exe";
    if (!FileExists(reportGeneratorExe) && !FileExists(reportGeneratorExeWin))
    {
        Information("Instalando dotnet-reportgenerator-globaltool em ./tools (será feito apenas uma vez)...");
        StartProcess("dotnet", "tool install --tool-path ./tools dotnet-reportgenerator-globaltool");
    }

    // Constrói lista de reports separados por ponto-e-vírgula
    var reportArgs = string.Empty;
    foreach (var f in reports)
    {
        if (!string.IsNullOrEmpty(reportArgs))
        {
            reportArgs = reportArgs + ";" + f.FullPath;
        }
        else
        {
            reportArgs = f.FullPath;
        }
    }

    Information("Gerando relatorio HTML em './CoverageReport' com ReportGenerator...");
    // Usa o executável instalado em ./tools/reportgenerator
    var rgPath = FileExists(reportGeneratorExeWin) ? reportGeneratorExeWin : reportGeneratorExe;
    StartProcess(rgPath, $"-reports:\"{reportArgs}\" -targetdir:\"{reportDir}\" -reporttypes:HtmlInline_AzurePipelines");
    Information($"Relatorio gerado em: {reportDir}");
});

// Default task depende de Test-Coverage (definido acima)

RunTarget("Default");