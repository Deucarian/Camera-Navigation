# Basic Navigation

Open BasicNavigation.unity and enter Play Mode. The scene provides a camera,
a DeucarianCameraNavigator, and one Unity Cube for pose capture,
perspective framing, animated movement, and top-down framing.

Use the Front / Side / Top / Close / Home buttons. Inspect **Navigation demo**
for the camera and renderer references, or **SampleMotionSettings.asset** for
transition speed, duration limits and easing. Settings are read on each command;
change them in the Inspector and try another move. The small sample controller
shows the corresponding code calls; it does not implement camera movement itself.

The package deliberately does not choose an input system. Feed camera input
from the input layer owned by your project.

The Cube material targets URP, declared as a package dependency. Assign a URP
pipeline asset in Graphics Settings and check Quality overrides; installation
alone does not activate that pipeline. For another renderer, replace the Cube's
material. Camera navigation behavior itself is render-pipeline agnostic.
