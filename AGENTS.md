# Filter Signals Agent Guide

Own only this repository. Classification must not mutate `ThingFilter` state.
Keep classification, cache invalidation, UI integration, compatibility
registration, settings, and navigation separated.

Navigation is additive and map-safe: act only on an unambiguous usable source
on the displayed map, global research, or an available vanilla Architect
designator. Missing/stale/custom targets fail without changing the filter or
camera. The status square owns tooltip behavior only inside its rectangle and
must clear an overlapping vanilla row tooltip without affecting other hover
behavior.

Use `SpineMod<FilterSignalsSettings>`, one Spine patch owner, the stable-tooltip
lease, and the inherited contextual lease. Bind status squares before their
navigation and controls before toggles. Keep the optional toolbar hidden by
default; colored status squares are primary.

Suppress RimWorld's inline small-volume marker only for the current row and
restore state on success and failure. Keep the replacement explanation
localized and concise.

Metadata, `Tools\CascadeManifest.json`, project files, and all files under
`Tests` define current support and verification. Follow
`A:\Dev\RimWorld\AGENTS.md` for shared build/runtime work and run every affected
executable and PowerShell boundary test.
