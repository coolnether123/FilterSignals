param([Parameter(Mandatory = $true)][string]$Phase, [Parameter(Mandatory = $true)][string]$Version)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repository = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$aboutXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ModMetaData>
  <name>Filter Signals</name>
  <author>CoolNether123</author>
  <packageId>CoolNether123.FilterSignals</packageId>
  <modVersion>1.0.0</modVersion>
  <supportedVersions>
    <li>$Version</li>
  </supportedVersions>
  <modDependencies>
    <li>
      <packageId>brrainz.harmony</packageId>
      <displayName>Harmony</displayName>
      <steamWorkshopUrl>steam://url/CommunityFilePage/2009463077</steamWorkshopUrl>
    </li>
    <li>
      <packageId>CoolNether123.Spine</packageId>
      <displayName>SpineLib</displayName>
      <steamWorkshopUrl>steam://url/CommunityFilePage/3778463813</steamWorkshopUrl>
      <downloadUrl>https://github.com/coolnether123/Spine</downloadUrl>
    </li>
  </modDependencies>
  <loadAfter>
    <li>brrainz.harmony</li>
    <li>CoolNether123.Spine</li>
  </loadAfter>
  <description>Shows what your colony can actually make, right inside RimWorld's item filters.

- A status square beside each item: producible, unlocked, locked, or unavailable
- Click a square to jump to a workstation, research project, or Architect option
- Optional filter toolbar, off by default
- Changing the view never touches your saved allow and deny choices

Requires Harmony and SpineLib.</description>
</ModMetaData>
"@
$loadFoldersXml = @"
<?xml version="1.0" encoding="utf-8"?>
<loadFolders>
  <v$Version>
    <li>/</li>
  </v$Version>
</loadFolders>
"@
if ($Phase -eq 'after-merge') {
    [System.IO.File]::WriteAllText((Join-Path $repository 'About\About.xml'), $aboutXml)
    [System.IO.File]::WriteAllText((Join-Path $repository 'LoadFolders.xml'), $loadFoldersXml)
    & git -C $repository add -- About/About.xml LoadFolders.xml
    if ($LASTEXITCODE -ne 0) { throw 'Could not stage support metadata.' }
}
elseif ($Phase -eq 'before-stage') {
    $assembly = [string](Get-Content -Raw -LiteralPath (Join-Path $repository 'Tools\CascadeManifest.json') | ConvertFrom-Json).build.expectedAssembly
    $source = Join-Path $repository "$Version\Assemblies\$assembly"
    $root = Join-Path $repository 'Assemblies'
    [System.IO.Directory]::CreateDirectory($root) | Out-Null
    [System.IO.File]::Copy($source, (Join-Path $root $assembly), $true)
    & git -C $repository add -- Assemblies About/About.xml LoadFolders.xml
    if ($LASTEXITCODE -ne 0) { throw 'Could not stage support payload.' }
}
