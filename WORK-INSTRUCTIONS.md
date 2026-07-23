# SPIE Ribbon - Work Instructions

Living spec and backlog. Source of truth for what gets built next, ahead of the code itself.
Read this in full before picking up any work here - especially the "For the scheduled agent"
section if you're an automated run with no memory of prior conversations.

## Current state (built and verified live in Revit 2024/2026 on the dev machine)

- One ribbon button ("SPIE Toolbox") opens a floating, resizable, modeless WPF window.
- **Module architecture is live**, not hardcoded tools:
  - `SpieRibbon.Contracts` - shared `ISpieModule` / `ISpieHost` / `ToolGroup` / `ToolItem`.
  - `SpieRibbon` (host) - discovers + loads modules from `%AppData%\SpieRibbon\Modules\<year>\`,
    renders category (module) > group > tool, owns the shared `ExternalEvent` Revit-context
    runner, owns settings (enable/disable per module, **disabled by default** for new modules).
  - `SpieRibbon.Algemeen` - real module. Groups: **Get Started** (Load RFA's, SPIE
    Handleidingen - open server folders), **Import/Export** (Export Schedule -> .xlsx via
    ClosedXML).
  - `SpieRibbon.Civil`, `SpieRibbon.EnI`, `SpieRibbon.Hvac` - placeholder modules, empty
    `BuildGroups()`.
- Multi-targeted per Revit version (net48 for 2024, net8.0-windows for 2025/2026) via
  `Directory.Build.props` + MSBuild configs `Debug/Release R24/R25/R26`, shared across all 6
  projects.
- Footer: "Created by MVS - Beta v0.1" (bump `Application.VersionLabel` when versioning up).
- Deploy: `deploy\Build-Package.ps1` builds all 3 versions and stages
  `dist\SpieRibbon-Package\` (host/<year>, modules/<year>, Deploy-SpieRibbon.ps1, Install.bat,
  README.txt), then zips it. `deploy\Deploy-SpieRibbon.ps1` installs per-user, no admin rights -
  writes `.addin` manifest + copies host/modules into `%AppData%`.
- Git: pushed to `https://github.com/MarkusSPIE/Test-v1.git`, branch `master`.

## Design conventions (apply these to all new work)

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
- **SPIE BIM Management** - NOT YET CREATED. Role-based, not discipline-based: model health,
  audits, cleanup, standards compliance. Most day-to-day engineers won't enable it; BIM
  managers/coordinators will. Same shape as the other modules (new project
  `SpieRibbon.BimManagement`, `ISpieModule`, disabled by default) - create it as part of
  implementing its first tool, following the Civil/EnI/Hvac placeholder pattern exactly.

## Backlog (pick items from here - not yet built)

Ordered roughly by value; not a strict sequence. Each item = one `ToolItem` (or a small set of
related ones) in the relevant module, using the group vocabulary above.

**SPIE Algemeen**
- Batch print / export sheets to PDF (group: Import/Export)
- Project info summary - phases, worksets, key project parameters at a glance (group: Get Started)

**SPIE BIM Management** (create the module + project as part of the first tool below)
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

## For the scheduled agent

You're picking this up with no memory of the conversations that produced this spec. Read this
whole file first - it's the only context you have.

1. **Pick the next backlog item** you're confident you can implement well. Skip ones needing
   domain input you don't have (currently: all of Civil/E&I/HVAC).
2. **Implement it as a real `ISpieModule` tool**: add a `ToolItem` to the right module (or
   create `SpieRibbon.BimManagement` following the Civil/EnI/Hvac placeholder pattern if it's a
   BIM Management item - copy their `.csproj` shape, add it to `SpieRibbon.sln` and to
   `moduleProjects` in `deploy\Build-Package.ps1`). Anything touching the Revit API must run via
   `host.RunInRevitContext(...)`, not called directly from a button click.
3. **Build-verify, don't skip this**: `dotnet build SpieRibbon.sln -c "Release R24"` (and R25,
   R26) must all succeed with 0 errors before you're done. This is the only verification you can
   do - you have no access to a real Revit session.
4. **You CANNOT live-test in Revit.** No Revit installation, no `G:\` network drive, no MCP
   Revit connector - those only exist on the maintainer's local machine. Do not claim a tool
   "works" - claim it "builds and is ready for a live-Revit verification pass."
5. **Push to a branch, not `master`** (e.g. `agent/warnings-report`), and open a PR. Never push
   directly to `master` - the maintainer batch-verifies queued PRs live in Revit periodically
   and merges what passes.
6. **Update this file**: move the item from Backlog to a new "Awaiting live verification"
   section (add one if it doesn't exist) with the PR link, so the maintainer knows what's queued.
7. If a tool genuinely can't be implemented without info you don't have (e.g. an exact server
   path, a discipline-specific formula), leave it in the backlog and say why in the PR
   description rather than guessing.

## Open items / not yet decided

- Contract-version mismatch handling: for now, assume host and modules are always built and
  deployed together (same package), so version skew shouldn't occur in practice. Revisit if
  modules are ever versioned/shipped independently of the host.
- Whether nested Expanders remain the right UI once there are 5 categories with real content
  in each - reassess after Civil/E&I/HVAC get their first real tools.
