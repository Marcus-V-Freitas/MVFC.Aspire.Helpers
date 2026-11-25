param([string]$BaselineVersion = '12.0.0')

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$runId = [Guid]::NewGuid().ToString('N')
$version = "12.0.1-compatibility.$runId"
$artifacts = Join-Path $root "artifacts/compatibility/$runId"
$packages = Join-Path $artifacts 'packages'
$baselines = Join-Path $artifacts 'baselines'
New-Item -ItemType Directory -Force $packages, $baselines | Out-Null
$names = 'CloudStorage', 'Mongo', 'Redis', 'RabbitMQ', 'GcpSpanner', 'WireMock'

Push-Location $root
try {
    foreach ($name in $names) {
        $id = "MVFC.Aspire.Helpers.$name"
        $lowerId = $id.ToLowerInvariant()
        $baseline = Join-Path $baselines "$lowerId.$BaselineVersion.nupkg"
        Invoke-WebRequest "https://api.nuget.org/v3-flatcontainer/$lowerId/$BaselineVersion/$lowerId.$BaselineVersion.nupkg" -OutFile $baseline -UseBasicParsing
        dotnet pack "src/$id/$id.csproj" --configuration Release --no-restore --output $packages `
            "-p:MinVerVersionOverride=$version" '-p:EnablePackageValidation=true' `
            "-p:PackageValidationBaselinePath=$baseline" '-p:ApiCompatEnableRuleCannotChangeParameterName=true' `
            '-p:ApiCompatEnableRuleAttributesMustMatch=true'
        if ($LASTEXITCODE -ne 0) { throw "Package/API validation failed for $id." }
        if (!(Test-Path (Join-Path $packages "$id.$version.nupkg"))) { throw "Expected local package was not produced for $id." }
    }

    $project = 'playground/MVFC.Aspire.Helpers.Compatibility/MVFC.Aspire.Helpers.Compatibility.csproj'
    dotnet restore $project "-p:CompatibilityPackageVersion=$version" "-p:RestoreAdditionalProjectSources=$packages"
    if ($LASTEXITCODE -ne 0) { throw 'Package consumer restore failed.' }

    foreach ($framework in 'net9.0', 'net10.0', 'net11.0') {
        dotnet run --project $project --configuration Release --framework $framework --no-restore "-p:CompatibilityPackageVersion=$version"
        if ($LASTEXITCODE -ne 0) { throw "Package consumption failed for $framework." }
    }
}
finally {
    Pop-Location
}
