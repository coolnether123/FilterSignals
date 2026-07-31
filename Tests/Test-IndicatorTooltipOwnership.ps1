param()

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$filterUi = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Source\Presentation\FilterUiController.cs'))

$indicator = [regex]::Match(
    $filterUi,
    '(?s)internal static void DrawIndicator\(.*?' +
    '(?=\s*private static void DrawToolbar)')
if (-not $indicator.Success)
{
    Write-Error 'Could not isolate the status-indicator UI boundary.'
    exit 1
}

$clear = $indicator.Value.IndexOf(
    'TooltipHandler.ClearTooltipsFrom(interactionRect)',
    [System.StringComparison]::Ordinal)
$tip = $indicator.Value.IndexOf(
    'TooltipHandler.TipRegion(',
    [System.StringComparison]::Ordinal)

if ($clear -lt 0 -or $tip -lt 0 -or $clear -gt $tip)
{
    Write-Error (
        'The status square must clear the overlapping vanilla tooltip ' +
        'before registering its own tooltip.')
    exit 1
}
if ($filterUi -notmatch
    'private const float StatusIndicatorRightInset = 61f' -or
    $indicator.Value -notmatch
    'listing\.ColumnWidth - StatusIndicatorRightInset')
{
    Write-Error (
        'The status-square column must leave RimWorld''s small-volume /10 ' +
        'marker unobstructed.')
    exit 1
}

Write-Output (
    'PASS: The status square owns its hover area without stacking the ' +
    'vanilla item tooltip or clipping the small-volume /10 marker.')
