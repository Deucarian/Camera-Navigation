# Changelog

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
