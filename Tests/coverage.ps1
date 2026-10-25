<#
.SYNOPSIS
Corre los tests con cobertura y genera el reporte HTML, todo dentro de Tests/TestResults.

.DESCRIPTION
1. Borra Tests/TestResults, para que el reporte no mezcle resultados de corridas anteriores.
2. Corre todos los tests de la solución con cobertura (Microsoft.Testing.Extensions.CodeCoverage).
   Cada proyecto de test deja su propio .cobertura.xml en Tests/TestResults.
3. Junta todos los .cobertura.xml y genera el reporte con ReportGenerator (herramienta local del
   repo, declarada en dotnet-tools.json) en Tests/TestResults/coverage-report:
   • Summary.txt: los porcentajes por ensamblado y por clase (lo primero que lee un agente).
   • Cobertura.xml: los hits de cada línea y branch, sumados de todos los proyectos de test y
     filtrados por -Assemblies (lo que lee un agente para ver qué quedó sin cubrir).
   • index.html: el reporte navegable, para una persona.

Tests/TestResults ya está ignorada por el .gitignore ([Tt]est[Rr]esult*/).

.PARAMETER Assemblies
Filtro de ensamblados de ReportGenerator (-assemblyfilters). Por defecto incluye todos los que
se midieron. Ejemplo para ver solo la capa Domain del módulo: "+Records.Persons.Domain".

.PARAMETER Open
Abre el reporte en el navegador al terminar.

.EXAMPLE
./Tests/coverage.ps1
./Tests/coverage.ps1 -Assemblies "+Records.Persons.Domain" -Open
#>
param(
    [string]$Assemblies = "+*",
    [switch]$Open
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$resultsDirectory = Join-Path $PSScriptRoot "TestResults"
$reportDirectory = Join-Path $resultsDirectory "coverage-report"

if (Test-Path $resultsDirectory) {
    Remove-Item -Recurse -Force $resultsDirectory
}

Push-Location $repoRoot
try {
    dotnet tool restore | Out-Null

    # Sin --coverage-output cada proyecto de test genera un archivo con nombre único: con un nombre
    # fijo, el segundo proyecto pisaría el XML del primero.
    dotnet test --solution Records.slnx --coverage --coverage-output-format cobertura --results-directory $resultsDirectory
    if ($LASTEXITCODE -ne 0) {
        throw "Fallaron los tests: no se genera el reporte de cobertura."
    }

    dotnet reportgenerator `
        "-reports:$resultsDirectory/*.cobertura.xml" `
        "-targetdir:$reportDirectory" `
        "-reporttypes:Html;TextSummary;Cobertura" `
        "-assemblyfilters:$Assemblies" `
        "-verbosity:Warning"
    if ($LASTEXITCODE -ne 0) {
        throw "ReportGenerator no pudo generar el reporte."
    }
}
finally {
    Pop-Location
}

Get-Content (Join-Path $reportDirectory "Summary.txt")

$reportIndex = Join-Path $reportDirectory "index.html"
Write-Host ""
Write-Host "Reporte: $reportIndex"

if ($Open) {
    Start-Process $reportIndex
}
