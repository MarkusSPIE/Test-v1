# SPIE Ribbon - Patch Notes

Shown in-app via Settings -> Patch notes. One heading per "big" version bump (a hand-edited
major/minor change in `deploy\VERSION.txt`), most recent first - not one per auto-bumped patch,
that would just be a build log. Write bullets for the person using the tool, not a developer:
what's new and why they'd care, not commit-message style.

## v0.1

- Module architecture: SPIE Algemeen, SPIE BIM Management, and placeholder Civil / E&I / HVAC
  modules, each independently enable/disable-able via Settings (new modules start disabled).
- Floating, resizable toolbox window with a vertical tab sidebar per category.
- Custom navy title bar (with SPIE's actual brand colors) on every window, replacing the
  native OS chrome - no more duplicated "SPIE Toolbox" text in two places.
- Renamed the product from "SPIE Toolbox" to "SPIE Ribbon" throughout.
- Per-tool version numbers, shown in each tool's tooltip.
- Tools added: Load RFA's, SPIE Handleidingen, Export Schedule (-> Excel), IFC Export (2GW).
- No-admin-rights install package (`Install.bat`) that detects installed Revit versions and
  deploys automatically.
- Auto-incrementing product version number on every packaged release.
- Patch notes viewer (this file, shown from Settings).
