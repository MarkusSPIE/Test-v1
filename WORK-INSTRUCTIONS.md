# SPIE Ribbon - Work Instructions

Living spec for the module/extension system. Updated as scope evolves; treat this as the
source of truth for what gets built next, ahead of the code itself.

## Current state (built and verified)

- One ribbon button ("SPIE Toolbox") opens a floating, resizable, modeless WPF window.
- Window shows two groups, each an `Expander`:
  - **Get Started** - "Load RFA's", "SPIE Handleidingen" (open server folders in Explorer)
  - **Import/Export** - "Export Schedule" (schedule picker -> .xlsx, via ExternalEvent)
- Footer: "Created by MVS - Beta v0.1"
- Everything compiled into a single `SpieRibbon.dll`, multi-targeted per Revit version
  (net48 for 2024, net8.0-windows for 2025/2026) via MSBuild configs `Debug/Release R24/R25/R26`.
- Deployed manually (no admin rights) via `deploy/Deploy-SpieRibbon.ps1`, which writes a
  per-user `.addin` manifest and copies the build to `%AppData%\SpieRibbon\<year>`.

## Target architecture: category > group > tool, via loadable modules

### Hierarchy

```
SPIE Toolbox (window)
  SPIE Algemeen (category / module)
    Get Started (group)
      Load RFA's
      SPIE Handleidingen
    Import/Export (group)
      Export Schedule
  SPIE Civil (category / module)      <- placeholder, no tools yet
  SPIE E&I (category / module)        <- placeholder, no tools yet
  SPIE HVAC (category / module)       <- placeholder, no tools yet
```

UI: one more nesting level of the same `Expander` pattern already in use (category Expander
containing group Expanders containing tool buttons). Revisit this pattern (e.g. a sidebar/tree
instead of nested expanders) only if it becomes visually unwieldy in practice.

### Assemblies

- **SpieRibbon.Contracts** - new class library. Defines the shared contract every module and
  the host compile against: `ISpieModule` (`Name`, `BuildGroups()`), plus the existing
  `ToolGroup` / `ToolItem` models (moved here from the host).
- **SpieRibbon** (host) - today's add-in. Becomes a thin loader: registers the ribbon button,
  owns the `ExternalEvent` plumbing modules need for Revit API access, and on toolbox-open
  discovers + loads modules and renders whatever's enabled. No longer hardcodes the tool list.
- **SpieRibbon.Algemeen**, **SpieRibbon.Civil**, **SpieRibbon.EnI**, **SpieRibbon.Hvac** - one
  class library per discipline, each implementing `ISpieModule`, each built against
  `SpieRibbon.Contracts`. Civil/E&I/HVAC start as placeholders (`BuildGroups()` returns an
  empty list, or a single "Coming soon" group) until real tools land.

Each module assembly is built per Revit version, same R24/R25/R26 configuration pattern as the
host - a module that ever needs Revit API access must match the host's target framework for
that version.

### Module discovery

- Host scans `%AppData%\SpieRibbon\Modules\<year>\*.dll` at toolbox-open time.
- Loads each via reflection, finds types implementing `ISpieModule`.
- A module that fails to load (missing dependency, exception, contract mismatch) is skipped
  with a logged/shown warning - it must never take down the other modules or the toolbox itself.

### Settings / enable-disable

- `%AppData%\SpieRibbon\settings.json` - per-user, survives redeploys (lives outside the
  overwritten `SpieRibbon\<year>` folder).
- Stores `{ "ModuleName": true/false }` for every module the host has ever discovered.
- **New modules default to disabled** until the user opts in via Settings (confirmed choice -
  a fresh install shows only SPIE Algemeen; colleagues enable Civil/E&I/HVAC themselves once
  those are relevant to them).
- A "Settings" button on the toolbox window opens a small dialog: checklist of every
  discovered module, current enabled state, OK/Cancel.
- **Enable/disable applies instantly.** All discovered modules are loaded into memory once at
  first toolbox-open regardless of enabled state; "enabled" is purely a display filter, so
  toggling a module re-renders the toolbox immediately (no restart needed).
- **What does need a Revit restart: newly *deployed* modules.** The Modules folder is scanned
  once per session. A module DLL added while Revit is running is not seen until restart. And we
  never *unload* a module assembly mid-session (true hot-unload is only feasible on the
  net8.0-windows builds via `AssemblyLoadContext`; net48/2024 would need a secondary AppDomain -
  not worth it). Documented as a deliberate boundary; revisit only if it becomes a real pain.

### Deploy script implications

`Deploy-SpieRibbon.ps1` needs to additionally copy each module's per-version build into
`%AppData%\SpieRibbon\Modules\<year>\`, alongside the existing host deployment. Same
no-admin, per-user-folder approach as today.

## Open items / not yet decided

- Contract-version mismatch handling: for now, assume host and modules are always built and
  deployed together (same package), so version skew shouldn't occur in practice. Revisit if
  modules are ever versioned/shipped independently of the host.
- Whether nested Expanders remain the right UI once there are 4 categories with real content
  in each - reassess after Civil/E&I/HVAC get their first real tools.
