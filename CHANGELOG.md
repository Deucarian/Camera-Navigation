# Changelog

## 0.2.9 - 2026-07-24

- Added generic posed-bounds geometry that converts position, rotation, and
  local bounds into deterministic world-space framing bounds.
- Enabled applications to frame data-backed spatial targets before their
  renderers or other asynchronous presentation resources exist.

## 0.2.8 - 2026-07-24

- Added a generic relaxed-distance framing profile with an editor-configurable
  relative distance multiplier for smaller or isolated targets.
- Kept standard composite framing unchanged while allowing applications to
  opt individual framing targets into the more distant profile.

## 0.2.7 - 2026-07-24

- Added a reusable automatic-framing profile with configurable target-rotation,
  padding, and near-clip clearance policies.
- Extended the existing categorized Camera Navigation settings window to
  create and edit canonical controls and framing assets together.
- Added coverage proving applications can preserve their current camera
  orientation while still fitting preferred-orientation targets.

## 0.2.6 - 2026-07-24

- Added immutable, orientation-aware framing targets that fit world bounds in
  the camera's current projection while preserving FoV and near-clip safety.
- Added focused coverage for perspective, orthographic, current-view fallback,
  preferred rotations, and invalid framing data.

## 0.2.5 - 2026-07-23

- Promoted the user-tuned Report Viewer Orbit and Fly profile to the package
  defaults used by new assets, runtime fallback controls, and reset actions.
- Added regression coverage for every raw profile value and its effective
  globally scaled navigation channels.

## 0.2.4 - 2026-07-23

- Exposed adjustable global sensitivity through an abstract controls contract.
- Defined the canonical runtime controls resource path used by project
  composition roots.
- Clarified that Global Sensitivity affects every navigation channel while
  Boost / Slow Scale applies only to modifier-driven translation.

## 0.2.3 - 2026-07-23

- Restored the Report Viewer's actual pre-extraction Orbit rotation, Orbit pan,
  and Fly look sensitivity defaults instead of using raw input constants.

## 0.2.2 - 2026-07-23

- Rebuilt the Camera Navigation settings window with the shared Deucarian
  Editor workbench, chrome, status, and workflow-control styling.
- Declared the exact Editor package dependency used by the editor-only
  assembly while keeping the runtime assembly editor-independent.

## 0.2.1 - 2026-07-23

- Restored the complete legacy Report Viewer Orbit and Fly speed profile as the
  package defaults.
- Made Orbit and Fly base speeds configurable through optional abstract speed
  profiles without breaking existing controls implementations.
- Added one categorized Camera Navigation editor window for creating, editing,
  selecting, and restoring project controls assets.

## 0.2.0 - 2026-07-23

- Made the Orbit minimum distance configurable and scale-aware.
- Included the camera near clip plane in the Orbit safety floor.
- Added pivot-safe smooth zoom that cannot cross or invert around the pivot.
- Added stable Orbit rotation, keyboard movement, and orthographic zoom.
- Added smooth Fly wheel dolly and normalized slow/boost modifiers.
- Added an application-implementable navigation controls interface.
- Expanded EditMode coverage for small and large model scales, projection modes,
  smoothing, and pivot stability.

## 0.1.1 - 2026-07-17

- Added the importable Basic Navigation sample and aligned the exact Common dependency.

## 0.1.0 - 2026-07-01

- Created the initial `com.deucarian.camera-navigation` package.
- Added camera pose, framing, motion settings, navigator, waypoint, orbit, fly, and input primitives.
