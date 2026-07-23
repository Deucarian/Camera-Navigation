# Deucarian Camera Navigation

## What this is

Deucarian Camera Navigation provides reusable Unity camera pose, framing, transition, orbit, fly, top-down, and waypoint movement primitives. It is input-agnostic by default: projects feed normalized input structs into the controllers or provide their own input adapters.

Package ID: `com.deucarian.camera-navigation`

Current package version: `0.2.5`.

## When to use it

- You need reusable camera framing and movement primitives without adopting an app-specific viewer stack.
- You want orbit/fly/top-down camera behavior that can be driven by your own input layer.
- You need camera poses and transitions that are safe to reuse across runtime features and tests.

## When not to use it

- You need a full app command system, report viewer, media viewer, or toolbar UI.
- You need a generic input package; this package intentionally stays input-agnostic.
- You need editor chrome or diagnostics views; those belong to other Deucarian packages.

## Install

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

## Unity compatibility

Requires Unity `2022.3` or newer.

## 60-second quick start

1. Add `DeucarianCameraNavigator` to a GameObject in the scene.
2. Assign the target `Camera`, or leave it empty to use `Camera.main`.
3. Create or edit an Orbit/Fly controls asset through
   `Tools > Deucarian > Experience and Interaction > World Interaction > Camera Navigation`.
4. Capture an origin pose and move to a top-down or framed pose from your own gameplay/viewer code:

```csharp
using Deucarian.CameraNavigation;
using UnityEngine;

public sealed class CameraNavigationExample : MonoBehaviour
{
    [SerializeField] private DeucarianCameraNavigator navigator;
    [SerializeField] private Renderer targetRenderer;

    private void Start()
    {
        if (navigator == null || targetRenderer == null)
        {
            return;
        }

        navigator.CaptureOrigin();
        navigator.MoveToTopDown(targetRenderer.bounds);
    }
}
```

## Samples

Import the **Basic Navigation** sample through Unity Package Manager for a
starting scene with camera navigation and top-down framing.

## Public API map

- `DeucarianCameraPose`: capture/apply camera transform and projection state.
- `DeucarianCameraMotionSettings`: transition speed, duration, projection-match field of view, and movement/rotation easing curves.
- `DeucarianCameraFraming`: bounds framing, top-down pose, projection matching, and clip-plane helpers.
- `DeucarianCameraNavigator`: cancellable move-to-pose, move-to-origin, top-down, and waypoint movement host.
- `DeucarianOrbitCameraController`: pivot-safe Orbit rotation, pan, movement, and smooth perspective/orthographic zoom.
- `DeucarianFlyCameraController`: Fly look, movement, modifiers, and smooth wheel dolly.
- `DeucarianCameraNavigationControls`: configurable Orbit/Fly base speeds,
  sensitivities, smoothing, modifiers, and Orbit minimum-distance policy.
- `IDeucarianCameraNavigationControls`: settings boundary for application-owned
  navigation control assets.
- `IDeucarianOrbitNavigationSpeeds` and `IDeucarianFlyNavigationSpeeds`:
  optional speed-profile boundaries that let application-owned controls override
  package base speeds without coupling to the concrete package asset.
- `DeucarianOrbitCameraInput` and `DeucarianFlyCameraInput`: normalized input values for any input backend.

### Orbit minimum distance

Perspective Orbit distance is clamped to the largest of:

- The configurable absolute minimum distance.
- Camera near clip plane multiplied by the configurable safety factor.
- Reference model radius multiplied by the configurable scale factor.

Call `SetReferenceBounds` or `SetReferenceScale` when content changes. This lets
small models zoom much closer while large models retain a stable scale-aware
safety floor. Orthographic navigation changes `orthographicSize` and does not
move the camera through its pivot.

## Integrations

Works with:

- `com.deucarian.common` for approved shared runtime primitives.
- `com.deucarian.editor` for the shared editor-only Camera Navigation settings
  surface.

Optional integrations:

- `com.deucarian.camera-navigation.input-system-integration` for configurable
  Unity Input System mouse/keyboard sources and a ready-to-use Orbit/Fly rig.

Does not own:

- Application command routing.
- Viewer bootstrap/report/media behavior.
- UI toolbar state.
- Generic input systems.

## Troubleshooting

- If `MoveToTopDown` does nothing, confirm the navigator has a target camera or the scene has a tagged `MainCamera`.
- If movement snaps, check whether the scene is in Edit Mode or the motion settings calculate a zero-duration transition.
- If framing looks too tight, pass a larger padding value to `DeucarianCameraFraming` helpers before moving to the pose.
- If Orbit stops sooner than expected, compare the configured absolute,
  near-clip, and reference-scale minimums with `GetMinimumDistance`.
- If Orbit or Fly feels too fast or slow, open the Camera Navigation window and
  adjust the complete speed profile or restore the approved Deucarian defaults.

## Validation

Run the shared package validator:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Run Unity EditMode tests when changing runtime code or asmdefs.

## Architecture / Contributor Notes

See [AGENTS.md](AGENTS.md) for ownership, dependency, and validation guidance.

## License

See [LICENSE.md](LICENSE.md).
