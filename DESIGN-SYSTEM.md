# SPIE Ribbon - Design System

Portable visual reference for SPIE Revit plugin UIs. Written to be copy-pasted as a starting
point into a *different* plugin's repo, not just describing this one - keep it self-contained.

## Brand colors

Extracted directly from `Logo_Spie.png` (sampled pixel values, not guessed):

| Role | Hex | Where it's used |
|---|---|---|
| Navy | `#003772` | Title bar background, category/group header text, icon tint, numbering digits |
| Red | `#E31C18` | Accent dot (echoes the logo's sun/globe mark), divider line under headers |

Logo source (local copy): `C:\Revit\SP\Logo_Spie.png` - also has DWG/PNG stamp variants under
that same folder if a plugin ever needs the full stamp set, not just the wordmark.

Don't introduce other brand colors without re-checking against the actual logo/brand
guidelines - these two were pixel-sampled, not eyeballed, so they're the source of truth.

## Custom title bar (every window)

The native OS window chrome is hidden entirely - every window uses a hand-built navy title bar
instead, so there's exactly one header, not a native one plus a custom one stacked on top of
each other (that duplication happened once already: OS chrome said "SPIE Toolbox" *and* the
custom header said it again underneath).

Implementation lives in the shared `SpieRibbon.Ui` project (namespace `SpieRibbon.Chrome` -
deliberately not `SpieRibbon.Ui`, which collides case-insensitively with the host's existing
`SpieRibbon.UI` window-code folder). Every window:

1. XAML: an empty top-docked `<Border x:Name="TitleBarHost"/>`, no `WindowStyle` set.
2. Code-behind, right after `InitializeComponent()`: `SpieChrome.Apply(this, TitleBarHost, "Title text")`.

`SpieChrome.Apply` hides the OS chrome (`WindowStyle=None` + `WindowChrome` for resize/drag/
snap behavior - `WindowChrome.CaptionHeight` gives drag-to-move and double-click-to-maximize for
free, no manual `DragMove()` wiring needed), draws the navy bar with the red accent dot and
title, and adds hand-built minimize/maximize/close buttons. It returns the button `StackPanel`
so a caller can insert an extra button (e.g. the main toolbox's Settings gear) before the
minimize/maximize/close group.

Deployment note: `SpieRibbon.Ui.dll` is deployed once, in the host folder (same pattern as
`SpieRibbon.Contracts.dll`) - modules reference it with `Private="false"`. `Application.OnStartup`
force-loads it early (`_ = typeof(SpieChrome);`) so it's resolvable before any module code runs,
the same reasoning as why `Host`/`Contracts` get wired up first in that method.

`SpieChrome.Apply` also wraps the window's content in a subtle 1px gray border and adds a
decorative resize-grip icon bottom-right. Native OS chrome normally gives every window a visible
edge/drop-shadow - `WindowStyle=None` loses that, so without the border a window can blend into
whatever's behind it. The grip is purely visual (`IsHitTestVisible=false`) - actual resizing from
that corner already works via `WindowChrome.ResizeBorderThickness`, so it must never be marked
`IsHitTestVisibleInChrome` or it would swallow the drag instead of letting WindowChrome handle it.
- **Categories** (one per module/discipline): navy, medium-weight header text, a thin red
  (`#E31C18`) underline/divider beneath it - not a full box border, just the bottom line.
  Collapsible (chevron).
- **Groups** (within a category): same collapsible pattern, unstyled/neutral text - only
  categories get the navy+red treatment, so the visual hierarchy stays readable (categories are
  the "loud" level, groups and tools are quieter).
- **Tools**: one row per tool - a small navy number (restarts at 1 per category, see below), an
  icon, then the label. Light border, no fill, rounded corners (~6px).
- **Empty state**: italic, muted gray "No tools available yet." - no border, no icon, sits where
  the tool rows would be.
- **Footer**: centered, small (10px), muted gray - "Created by [initials] - [version label]".

## Every window, not just the main toolbox

Easy mistake (made it once already): applying this design system to the main toolbox window
and then leaving every *sub*-dialog (Settings, a tool's own picker/options window, etc.) in
plain default WPF styling. Every window the plugin ever shows gets the same title bar pattern -
navy background, red accent dot, white title text - even small utility dialogs. When adding any
new `Window`, check it against this file before considering it done, and specifically re-check
every existing window whenever this design system itself changes.

## Numbering

- Restarts at **1 for every category** (not a running count across the whole toolbox) - each
  category is a self-contained section, so its numbering is too.
- Shown as a small navy digit immediately before the tool's icon, not after the label.

## Icons

- One icon per tool, to the right of its number.
- Native app: use **Segoe MDL2 Assets** (not the newer Segoe Fluent Icons) - ships on every
  Windows 10/11 machine, whereas Fluent Icons needs a Windows build a colleague might not have.
  No custom asset creation needed. Codepoints for common glyphs are shared between the two
  fonts, so this stays correct if Fluent Icons is ever adopted later. Tint icons navy
  (`#003772`) to match the header color, not the default system color. Embed codepoints via
  `char.ConvertFromUtf32(0xNNNN)` in C#, not `"\uNNNN"` string literals or raw pasted glyphs -
  both have proven unreliable to write/transmit correctly.
- Mockups/previews (e.g. in chat): Tabler outline icons are a fine stand-in when discussing
  layout before wiring up the real glyph font.
- Pick an icon that represents the *action*, not the category (e.g. a folder icon for "open a
  folder" tools, a spreadsheet icon for exports, a building icon for anything IFC/model-related).

## Reusing this elsewhere

If starting a new SPIE plugin: reuse the navy/red pair and the title-bar + category-divider
pattern as-is for instant visual consistency across SPIE's internal tools. The
numbering-per-category and empty-state conventions are more about UX predictability than
branding - worth keeping if the new plugin has a similar "grouped list of tools" shape, safe to
drop if it doesn't.
