param()

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$controller = [System.IO.File]::ReadAllText(
    (Join-Path $root `
        'Source\Presentation\ClassificationNavigationController.cs'))
$filterUi = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Source\Presentation\FilterUiController.cs'))
$presentation = [System.IO.File]::ReadAllText(
    (Join-Path $root `
        'Source\Presentation\ClassificationPresentation.cs'))
$language = [System.IO.File]::ReadAllText(
    (Join-Path $root `
        'Languages\English\Keyed\FilterSignals.xml'))

$failures = [System.Collections.Generic.List[string]]::new()
$buildOption = [regex]::Match(
    $controller,
    '(?s)private static bool SelectBuildOption\(.*?' +
    '(?=\s*}\s*}\s*})')
if (-not $buildOption.Success)
{
    $failures.Add('Could not isolate Architect navigation.')
}
elseif ($buildOption.Value -match 'CameraJumper')
{
    $failures.Add(
        'Architect navigation must not move or recenter the camera.')
}
if ($controller -notmatch 'Find\.CurrentMap\s*!=\s*target\.Map' -or
    $controller -notmatch 'Find\.CurrentMap\s*!=\s*source\.Map')
{
    $failures.Add(
        'Navigation must fail safely for a non-current target or source map.')
}
elseif ($buildOption.Value -notmatch
    'target\.BuildDesignator\.Visible[\s\S]*?' +
    'Find\.DesignatorManager\.Select\(target\.BuildDesignator\)')
{
    $failures.Add(
        'Architect navigation must use the designator resolved by the ' +
        'same actionable target that supplied the tooltip.')
}
if ($controller -notmatch
    'target\.Research\.IsFinished' -or
    $controller -notmatch
    'target\.BuildDesignator\.PlacingDef\s*!=\s*target\.Buildable')
{
    $failures.Add(
        'Click handling must revalidate research freshness and the build target.')
}
if ($filterUi -notmatch
    '(?s)target\.IsActionable\s*\?' +
    '[\s\S]*?ClassificationPresentation\.NavigationTooltip')
{
    $failures.Add(
        'Tooltip navigation prose must use the same actionability as clicks.')
}
$resolver = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Source\Runtime\ClassificationNavigationResolver.cs'))
if ($resolver -notmatch
    '(?s)CollectBuildDesignators\(' -or
    $resolver -notmatch
    '(?s)SelectValidatedBuildDesignator\(matches\)' -or
    $resolver -notmatch
    '(?s)matches\.Count\s*!==?\s*1' -or
    $resolver -notmatch
    '(?s)runtimeType\s*==\s*typeof\(Designator_Build\)' -or
    $resolver -notmatch
    '(?s)runtimeType\.Assembly\s*==\s*typeof\(Designator_Build\)\.Assembly')
{
    $failures.Add(
        'Architect selection must reject duplicates and non-standard ' +
        'designator runtime types.')
}

function Resolve-TestArchitectDesignator([object[]]$candidates)
{
    $matching = @($candidates | Where-Object {
        $_.Visible -and $_.PlacingDef -eq 'Target'
    })
    if ($matching.Count -ne 1)
    {
        return $null
    }

    $candidate = $matching[0]
    if ($candidate.RuntimeType -ne 'RimWorld.Designator_Build' -or
        $candidate.DeclaringAssembly -ne 'Assembly-CSharp')
    {
        return $null
    }

    return $candidate
}

$customOnly = Resolve-TestArchitectDesignator @(
    [pscustomobject]@{
        Name = 'CustomBuildDesignator'
        RuntimeType = 'Mod.CustomBuildDesignator'
        DeclaringAssembly = 'CustomMod'
        Visible = $true
        PlacingDef = 'Target'
    })
if ($null -ne $customOnly)
{
    $failures.Add(
        'A custom-only Architect designator must not become actionable.')
}

$standard = Resolve-TestArchitectDesignator @(
    [pscustomobject]@{
        Name = 'StandardBuildDesignator'
        RuntimeType = 'RimWorld.Designator_Build'
        DeclaringAssembly = 'Assembly-CSharp'
        Visible = $true
        PlacingDef = 'Target'
    })
if ($null -eq $standard -or $standard.Name -ne 'StandardBuildDesignator')
{
    $failures.Add(
        'The standard RimWorld Designator_Build must remain actionable.')
}

$duplicate = Resolve-TestArchitectDesignator @(
    [pscustomobject]@{
        Name = 'StandardOne'
        RuntimeType = 'RimWorld.Designator_Build'
        DeclaringAssembly = 'Assembly-CSharp'
        Visible = $true
        PlacingDef = 'Target'
    },
    [pscustomobject]@{
        Name = 'StandardTwo'
        RuntimeType = 'RimWorld.Designator_Build'
        DeclaringAssembly = 'Assembly-CSharp'
        Visible = $true
        PlacingDef = 'Target'
    })
if ($null -ne $duplicate)
{
    $failures.Add(
        'Multiple matching Architect designators must remain non-actionable.')
}
$indicator = [regex]::Match(
    $filterUi,
    '(?s)internal static void DrawIndicator\(.*?' +
    '(?=\s*private static void DrawToolbar)')
if (-not $indicator.Success)
{
    $failures.Add(
        'Could not isolate status-square click handling.')
}
elseif ($indicator.Value -match 'ClearSelection\(\)')
{
    $failures.Add(
        'A square with no action must leave the storage panel open.')
}
elseif ($indicator.Value -notmatch
    '(?s)ButtonInvisible\(interactionRect\).*?' +
    'if \(target\.IsActionable\).*?' +
    'ClassificationNavigationController\.TryNavigate\(target\)')
{
    $failures.Add(
        'Status-square navigation must run only for an actionable target.')
}
if ($presentation -match 'NavigationUnavailable' -or
    $presentation -match 'NavigationChoiceRule' -or
    $presentation -match 'SelectedPath' -or
    $presentation -match 'IndexOf\(')
{
    $failures.Add(
        'Presentation must use structured causes and must not parse English ' +
        'or explain unavailable navigation/internal target-selection rules.')
}
if ($presentation -notmatch
    'default:\s*\r?\n\s*return string\.Empty')
{
    $failures.Add(
        'A square with no navigation action must add no navigation text.')
}
if ($language -notmatch
    '<FilterSignals_CanMake>Able to be produced in this colony</FilterSignals_CanMake>')
{
    $failures.Add(
        'A producible item must use the requested concise colony wording.')
}
$canMakeExplanation = [regex]::Match(
    $presentation,
    '(?s)case ProductionClassification\.CanMakeNow:\s*' +
    'return string\.Empty;')
if (-not $canMakeExplanation.Success)
{
    $failures.Add(
        'A producible item must not add a redundant explanation line.')
}

if ($failures.Count -gt 0)
{
    foreach ($failure in $failures)
    {
        Write-Error $failure
    }
    exit 1
}

Write-Output (
    'PASS: Architect navigation preserves the camera, no-target clicks ' +
    'leave storage open, and tooltips omit unavailable-action prose.')
