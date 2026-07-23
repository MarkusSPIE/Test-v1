# SPIE Ribbon - Patch Notes

Shown in-app via Settings -> Patch notes. Add an entry here whenever you bump
`deploy\VERSION.txt` for a real package - most recent first.

## v0.1 (unreleased patches since)

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
