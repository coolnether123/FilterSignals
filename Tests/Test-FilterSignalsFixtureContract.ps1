$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$manifest = Get-Content -Raw -LiteralPath (Join-Path $root 'Tools\FixtureManifest.json') | ConvertFrom-Json
$expected = @('1.6', '1.5', '1.4', '1.3', '1.2', '1.1')

if ((@($manifest.supportedVersions) -join ',') -cne ($expected -join ','))
{
    throw 'Fixture manifest version order must match the supported runtime matrix.'
}

foreach ($version in $expected)
{
    $contract = $manifest.uiContracts.$version
    if ([string]::IsNullOrWhiteSpace($contract))
    {
        throw "Fixture UI contract is missing for $version."
    }
}

$source = Get-Content -Raw -LiteralPath (Join-Path $root 'Developer\FilterSignals.TestFixture\Source\FilterSignalsDebugActions.cs')
$project = Get-Content -Raw -LiteralPath (Join-Path $root 'Developer\FilterSignals.TestFixture\Source\FilterSignals.TestFixture.csproj')
foreach ($token in @('fixtureState', 'explanationCoverage', 'cacheProbe', 'FinishResearchProject'))
{
    if ($source -notmatch [regex]::Escape($token))
    {
        throw "Fixture observation contract is missing: $token"
    }
}

if ($source -notmatch 'FILTER_FIXTURE_MODERN_UI' -or
    $project -notmatch 'FILTER_FIXTURE_MODERN_UI' -or
    $project -notmatch 'FILTER_FIXTURE_LEGACY_UI' -or
    $source -notmatch 'FILTER_FIXTURE_LUDEON_TK' -or
    $project -notmatch 'FILTER_FIXTURE_HAS_RECIPE_WORKER_PART' -or
    $source -notmatch 'FILTER_FIXTURE_HAS_STORABLE_FILTER_FACTORY')
{
    throw 'Fixture UI and runtime API differences must be explicitly version-gated.'
}

Write-Output 'PASS: Filter Signals fixture contract covers every supported version.'
