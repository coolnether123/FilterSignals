# Duplicate check

Checked 2026-07-30 against current Steam Workshop, GitHub, public RimWorld
modding results, and behavior synonyms. No exact maintained equivalent or
package-ID collision was found. The original Discord thread was not publicly
identifiable, so no author identity is guessed.

Closest adjacent projects:

- [Crafted Locally Filter](https://steamcommunity.com/sharedfiles/filedetails/?id=3456812600)
  adds a binary locally/not-locally-crafted filter. It does not classify
  research, facilities, pawn capability, or multiple production paths.
- [WeaponStats](https://steamcommunity.com/sharedfiles/filedetails/?id=974066449)
  offers a craftable toggle in a separate weapon/apparel comparison UI.
- [Research Tree (Continued)](https://steamcommunity.com/sharedfiles/filedetails/?id=3030499331)
  exposes research unlocks but not current colony production capability in
  `ThingFilter` dialogs.
- [Filter Manager](https://steamcommunity.com/sharedfiles/filedetails/?id=2812197851)
  saves static filter presets rather than dynamic colony-aware classifications.

Decision: proceed. Filter Signals remains distinct through its four-state
production-path model, targeted invalidation, explanations, non-mutating UI
semantics, and compatibility API.
