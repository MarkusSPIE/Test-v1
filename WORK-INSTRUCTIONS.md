# SPIE Ribbon - Work Instructions

Living spec and backlog. Source of truth for what gets built next, ahead of the code itself.

See `DESIGN-SYSTEM.md` for the toolbox's visual design (brand colors, layout pattern, numbering,
icons) - written to be portable to other SPIE plugins, not just this one.

## How we work

No scheduled/automated agent - deliberately decided against it, since live-Revit verification
can't be automated anyway (needs this machine's actual Revit installs, the `G:\` network drive,
and the MCP Revit connector, none of which exist outside the maintainer's PC). Instead: chat
through the next item(s) to confirm scope, list exactly what's about to be built, then build it
(write -> build-verify all 3 configs -> maintainer live-tests in Revit).

## Current state (built and verified live in Revit 2024/2026 on the dev machine)

- One ribbon button ("SPIE Ribbon" - renamed from "SPIE Toolbox") opens a floating, resizable,
  modeless WPF window with a custom navy title bar (native OS chrome is hidden entirely - see
  DESIGN-SYSTEM.md's "Custom title bar" section).
- **Module architecture is live**, not hardcoded tools:
  - `SpieRibbon.Contracts` - shared `ISpieModule` / `ISpieHost` / `ToolGroup` / `ToolItem`
    (`ToolItem.Version`, e.g. `"v0.1"` - set explicitly on every tool, shown in its tooltip).
  - `SpieRibbon.Ui` (namespace `SpieRibbon.Chrome`) - shared custom-title-bar helper
    (`SpieChrome.Apply`) and brand color brushes (`SpieColors`), used by the host and every
    module's own dialogs. Deployed once in the host folder, same pattern as Contracts.dll.
  - `SpieRibbon` (host) - discovers + loads modules from `%AppData%\SpieRibbon\Modules\<year>\`,
    renders category (module) > group > tool via a vertical tab sidebar, owns the shared
    `ExternalEvent` Revit-context runner, owns settings (enable/disable per module, **disabled
    by default** for new modules).
  - `SpieRibbon.Algemeen` - Get Started (Load RFA's, SPIE Handleidingen - open server folders),
    Import/Export (Export Schedule -> .xlsx via ClosedXML).
  - `SpieRibbon.BimManagement` - Import/Export (**IFC Export (2GW)** - pick IFC settings +
    3D views, exports each view to `<ViewName>.ifc`, setting Project Information's Building Name
    to match the view's name before each export; Building Name is left at the last exported
    view's value, not restored). "2GW" = a project number, not a fixed abbreviation - the label
    can be genericized later if this tool turns out to be reused beyond that project.
  - `SpieRibbon.Civil`, `SpieRibbon.EnI`, `SpieRibbon.Hvac` - placeholder modules, empty
    `BuildGroups()`.
- Multi-targeted per Revit version (net48 for 2024, net8.0-windows for 2025/2026) via
  `Directory.Build.props` + MSBuild configs `Debug/Release R24/R25/R26`, shared across all 8
  projects.
- Footer: "Created by MVS - Beta v{major}.{minor}.{patch}" - see "Versioning" below for how this
  number moves, separate from each tool's own `ToolItem.Version`.
- Deploy: `deploy\Build-Package.ps1` builds all 3 versions and stages
  `dist\SpieRibbon-Package\` (host/<year>, modules/<year>, Deploy-SpieRibbon.ps1, Install.bat,
  README.txt), then zips it. `deploy\Deploy-SpieRibbon.ps1` installs per-user, no admin rights -
  writes `.addin` manifest + copies host/modules into `%AppData%`.
- Git: pushed to `https://github.com/MarkusSPIE/Test-v1.git`, branch `master`.

## Versioning

Two independent version numbers - don't confuse them:

**Product version** (shown in the toolbox footer, format `Beta v{major}.{minor}.{patch:D2}`,
e.g. `Beta v0.1.01`):
- Source of truth is `deploy\VERSION.txt` (plain `major.minor.patch` line).
- **Patch auto-bumps every time `Build-Package.ps1` runs** (without `-SkipBuild`) - it increments
  the patch number in `VERSION.txt`, then regenerates `src\SpieRibbon\VersionInfo.g.cs` (a
  committed, generated file - never hand-edit it) before building. This means every real
  distributed package has a distinct, traceable version automatically - no step to remember.
- **For a "big" update**, manually edit `deploy\VERSION.txt` - bump major or minor, reset patch
  to `0`. The next `Build-Package.ps1` run picks it up from there (e.g. `0.1.7` -> hand-edit to
  `0.2.0` -> next package run produces `Beta v0.2.01`).
- Patch bumps on *packaging*, not on every commit or dev build - a version number should mean
  "this specific thing got handed to someone," not "some code changed."

**Per-tool version** (`ToolItem.Version`, shown in that tool's tooltip, e.g. `"v0.1"`):
- **Manual only, never auto-bumped.** A tool's version means "this specific tool was changed" -
  bump it only when you actually modify that tool's code, as part of that change. Leave every
  other tool's version untouched when you do.
- Every `ToolItem` must set `Version` explicitly (don't rely on the Contracts default silently) -
  see DESIGN-SYSTEM.md's "Every tool gets a version".

**Patch notes** (`CHANGELOG.md`, shown in-app via Settings -> Patch notes):
- **Every "big" version bump** (a hand-edited major/minor change in `deploy\VERSION.txt`) **gets
  a bullet-point summary added to `CHANGELOG.md` as part of making that change** - not left for
  later, same as updating this file's "Current state" section. Write it for the person using the
  tool, not a developer: plain language, what's new and why they'd care - not commit-message
  style, not implementation detail.
- Routine auto-bumped patches don't need their own entry - they fold under the current
  big-version heading. The changelog should read as "what's new since the last real update," not
  a build log of every package that ever went out.
- New heading per big version (e.g. `## v0.2`), most recent first, bullets underneath.

## Design conventions (apply these to all new work)

### Every tool gets a version

Add `Version = "v0.1"` explicitly to every new `ToolItem` (don't rely on the contract's default
silently). Bump it when a tool changes meaningfully - it's shown in the tooltip, useful for
"which version of this tool do you have" when troubleshooting with a colleague.

### Group naming is a shared vocabulary across every module

A group name means the same kind of thing in every category, so users build one mental model
across disciplines. Reuse these; only add a new group name if a tool genuinely doesn't fit:

- **Get Started** - onboarding, reference links, templates, standards docs
- **Import/Export** - schedule/data exports, format conversions
- **Modeling** - tools that place/create/modify elements
- **QA/QC** - checks, audits, validation, warnings triage
- **Cleanup** - purge, standardization, housekeeping

### Category = who it's for (discipline or role), not what it does

- **SPIE Algemeen** - stuff every engineer uses regardless of discipline.
- **SPIE Civil / SPIE E&I / SPIE HVAC** - discipline-specific engineering tools.
- **SPIE BIM Management** - role-based, not discipline-based: model health, audits, cleanup,
  standards compliance, IFC export. Most day-to-day engineers won't enable it; BIM
  managers/coordinators will.

## Backlog (pick items from here - not yet built)

Ordered roughly by value; not a strict sequence. Each item = one `ToolItem` (or a small set of
related ones) in the relevant module, using the group vocabulary above. Deliberately building
one at a time rather than all at once - chat through scope before implementing each.

**SPIE Algemeen**
- Batch print / export sheets to PDF (group: Import/Export)
- Project info summary - phases, worksets, key project parameters at a glance (group: Get Started)

**SPIE BIM Management**
- Warnings report - export all Revit warnings to Excel with element IDs/categories, for triage (Import/Export)
- Purge unused, with a report of what got removed (Cleanup)
- Model health check - counts of families/sheets/views/CAD links/in-place families vs.
  reasonable thresholds (QA/QC)
- Unused views & sheets finder - views not on any sheet, sheets with nothing on them (QA/QC)
- Linked model / broken link report (QA/QC)
- Naming convention checker - sheet numbers/view names against a pattern (QA/QC)
- Batch parameter setter - set a parameter across a whole category via a picker (Modeling) -
  worth prioritizing, it's a reusable foundation other tools can build on

**SPIE Civil / SPIE E&I / SPIE HVAC**
- No concrete tools chosen yet - these need input from someone who actually does that
  discipline's daily work, not guesses. Each will likely want at least one Import/Export
  (schedule/data export) and one QA/QC tool to start, following the same group vocabulary.

## Open items / not yet decided

- Contract-version mismatch handling: for now, assume host and modules are always built and
  deployed together (same package), so version skew shouldn't occur in practice. Revisit if
  modules are ever versioned/shipped independently of the host.
- Whether nested Expanders remain the right UI once there are 5 categories with real content
  in each - reassess after Civil/E&I/HVAC get their first real tools.
- Whether "IFC Export (2GW)" should be renamed/genericized if it turns out to be useful beyond
  the one project it's currently named after.
