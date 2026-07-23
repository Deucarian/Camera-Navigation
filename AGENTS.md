# Deucarian Camera Navigation Agent Notes

Package ID: `com.deucarian.camera-navigation`
Repository: `Deucarian/Camera-Navigation`

Follow the canonical Deucarian governance docs in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/main/ARCHITECTURE.md), especially capability ownership and dependency rules.

## Ownership

This package owns:

- Camera pose, framing, transition, orbit, fly, top-down, and waypoint navigation primitives.

Registered capabilities:
- `camera-navigation`

This package must not own:

- Generic input systems, application command routing, viewer bootstrap, report/media behavior, UI toolbar state, or copied Unity object cleanup helpers.

## Dependencies

Allowed dependency shape:

- May depend on Common for approved Unity object cleanup and low-level runtime primitives.

Required dependencies and why:

- `com.deucarian.common`: approved Unity object lifetime helper and shared runtime primitive owner.

Optional/version-defined dependencies:

- None.

Architecture exceptions:

- None.

## Policies

- Logging: Do not add diagnostics/logging unless package behavior actually needs it; if needed, use Deucarian Logging and update all metadata together.
- Common: Use Common-owned helpers instead of local copies when production code needs approved shared cleanup/runtime primitives.
- Editor UI: No editor shell or shared editor UI ownership.
- Diagnostics: Do not become Diagnostics; camera navigation may expose status through its own APIs only when needed.
- Testing: Test fixture teardown may use `DestroyImmediate` directly.

## Validation

Run the shared validator before committing:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Also run existing repository tests when changing code or asmdefs. Documentation-only updates should still run `git diff --check`.

## Codex Guidance

- Inspect current files before changing anything.
- Work on `develop`; do not edit or merge `main` unless the task is promotion-only.
- Do not edit `Library/PackageCache`.
- Do not guess package versions or dependency versions.
- Do not add package dependencies casually; update asmdefs, `package.json`, `deucarian-package.json`, Package Registry, Package Installer fallback catalog, and Bootstrap fallback catalog together when a dependency is truly required.
- Do not create local copies of shared helpers.
- Keep commits focused and report exactly what changed and what was validated.

## Before Adding Code

- Confirm the change fits this package's ownership boundary.
- Reuse existing local patterns and helpers.
- Avoid broad refactors without audit support.
- Preserve runtime behavior unless the task explicitly asks to change it.

## Before Adding A Dependency

- Is the capability already owned by that package?
- Is it used by production code, editor code, sample code, or tests?
- Does the asmdef reference match `package.json`?
- Does `deucarian-package.json` need updating?
- Does Package Registry need updating?
- Does Package Installer fallback catalog need updating?
- Does Bootstrap fallback catalog need updating?
- Are exact versions propagated without guessing?

## Before Adding A Helper

- Is this package the capability owner?
- Is this behavior repeated in at least three production packages?
- Is there an existing owner package?
- Should this remain local?
- Has the audit been updated?

## Debug And Unity Object Lifetime

- Direct Unity Debug calls are forbidden in production code.
- Production Unity object cleanup must use Common-owned cleanup APIs when cleanup is needed.
- Test fixture teardown may use `DestroyImmediate` directly.
