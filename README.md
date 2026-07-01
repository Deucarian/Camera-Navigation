# Deucarian Camera Navigation

## Overview

Deucarian Camera Navigation provides reusable camera pose, framing, transition, orbit, fly, top-down, and waypoint movement primitives.

Package ID: `com.deucarian.camera-navigation`

Current package version: `0.1.0`.

The package is input-agnostic by default. Apps feed normalized structs into the controllers or use their own input adapters.

## Installation

Install through Unity Package Manager with a Git URL:

```json
{
  "dependencies": {
    "com.deucarian.camera-navigation": "https://github.com/Deucarian/Camera-Navigation.git#main"
  }
}
```

For development builds, use:

```json
"com.deucarian.camera-navigation": "https://github.com/Deucarian/Camera-Navigation.git#develop"
```

## Public API

- `DeucarianCameraPose`: capture/apply camera transform and projection state.
- `DeucarianCameraMotionSettings`: transition speed, duration, projection-match field of view, and movement/rotation easing curves.
- `DeucarianCameraFraming`: bounds framing, top-down pose, projection matching, and clip-plane helpers.
- `DeucarianCameraNavigator`: cancellable move-to-pose, move-to-origin, top-down, and waypoint movement host.
- `DeucarianOrbitCameraController` and `DeucarianFlyCameraController`: input-agnostic camera control primitives.

## Notes

This package does not own app command parsing, viewer bootstrap, report/media behavior, or UI toolbar state.
