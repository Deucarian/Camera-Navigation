# Changelog

## Asset workflow — Unreleased

- Add an isolated ground grid beneath the real preview cube, using the active render pipeline and shared editor colors. Viewer Navigation inherits the same preview; framing bounds and scene objects remain unchanged.

- Bundle controls and framing defaults matching runtime values, replace arbitrary-first discovery with canonical project/bundled resolution and retain framing selection across redraws.

## [0.4.0] - Unreleased

- Add typed reusable definition authoring and/or scoped Inspector components that share the existing C# service behavior.
- Include a playable Definition Workflow sample with configured hosts, short callers and usage documentation.
- Align declared package dependencies with the definition-authoring development wave.

- Render an actual Unity Cube in an isolated preview scene, using the active pipeline's default material and the existing smooth navigation controls.
- Simplify the importable scene to one Cube and declare its URP shader dependency; document pipeline setup separately from package installation.
- Replace the placeholder sample with an assigned camera, visible reference geometry, editable motion settings and working pose/framing commands.
- Declare the built-in IMGUI module used only by sample controls; keep input-system ownership outside the navigator.


## [0.3.0] - 2026-09-11

- Replace the illustrative auto-rotation with an interactive isolated camera driven by the runtime Orbit/Fly controllers, wheel damping, framing, and transitions.
- Apply live controls to the same preview camera, support focused keyboard/pointer input, and release all owned objects and input when closing the page.
- Add an explicit manual transition clock for isolated Edit Mode previews and deterministic hosts; automatic runtime behavior stays the default.
- Require Editor 1.11.0 for camera-projected shared preview geometry.
- Match the visible target framing when animated navigation switches perspective/orthographic projection, using the same top-down policy as Viewer Navigation.
- Fix default wheel zoom collapsing to the pivot: Orbit and Fly now share a bounded, reversible distance scale per detent (about 33% inward at defaults), with fractional/accumulated input and existing smoothing/minimums preserved.
- Normalize preview pointer/wheel units like the runtime adapter; preserve the current pose when gestures interrupt framing and avoid idle preview geometry rebuilds.
- Integrate the typed preset/awaitable move APIs with the manual clock, preserving cancellation generations and completion callbacks; remove ignored-root `Samples~` metadata that caused asset-refresh warnings.

## [0.2.15] - 2026-09-11

- Use a native camera navigation form and isolated spatial specimen; retain target selection, existing settings and per-page state.
- Require Editor 1.10.6 for the shared native controls, typography, responsive layouts and accessible interaction states.

## [0.2.14] - 2026-09-09

### Changed

- Adopt the shared Editor 1.7 workspace presentation: neutral surfaces, readable typography, consistent actions and aligned controls.
- Preserve package workflows and native serialized editing; this is an editor-only presentation update.

## [0.2.13] - 2026-09-09

- Register package tooling and navigation actions as shared Control Center pages. Preserve the domain workflow while using Editor-owned submenus, in-window navigation, and UI scaling.

## 0.2.12 - 2026-08-31

- Registered the package workflow and a bounded, sanitized local-state card with Deucarian Control Center.
- Removed normal `Tools/Deucarian` menu exposure while preserving the standalone open API.
- Updated the shared Editor dependency to 1.2.0.

## 0.2.11 - 2026-08-26

- Moved the package workflow to the direct capability menu
  `Tools/Deucarian/Camera Navigation`.
- Added regression coverage that rejects the former nested viewer-oriented
  categorization.
- Updated the exact Editor dependency to 1.1.0 for installed-metadata footer
  resolution.

## 0.2.10 - 2026-07-24

- Added a generic framing-bounds strategy contract so applications can select
  their bounds source through composition instead of conditional policy.
- Added reusable posed-data and rendered-geometry strategies backed by the
  existing pure bounds calculations.

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
