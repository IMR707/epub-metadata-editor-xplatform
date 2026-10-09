# Cross-platform implementation plan

Planning baseline: upstream version 2.0.4, commit `bc54c67`. Prepared 2026-10-09 for `IMR707/epub-metadata-editor-xplatform`.

The direction is a gradual C# reimplementation using Avalonia and modern .NET, with C# throughout the core, desktop interface, CLI, and tests. The original VB.NET source remains a reference; useful algorithms will be ported selectively and verified with regression tests. The first public release must have full feature parity with the existing application, including batch tools, renaming, internal file editing, and previews. Intermediate milestones are development checkpoints, not reduced-scope releases.

## Branch and release policy

- Develop the port on `port/cross-platform`; use short-lived feature branches from it when useful.
- Keep `master` at the latest stable release. Until the first cross-platform release is ready, it retains the original Windows baseline. Merge the port into `master` only after the full-parity release criteria pass, and tag that release commit.
- Maintain the existing GitHub fork relationship, original history, and upstream release tags. Keep the `upstream` remote to follow the original project; review and selectively adapt relevant changes as the implementations diverge.
- Give XPlatform independent semantic release numbers: `0.x.y` for internal development milestones, `1.0.0-beta.1` for full-parity verification builds, and `1.0.0` for the first public full-parity release. Development and verification builds do not relax the first public release's feature requirements.
- Use `xplatform-v<version>` for XPlatform Git tags, such as `xplatform-v1.0.0`. Display the unprefixed version in the application. Retain upstream version 2.0.4 as provenance rather than continuing its release sequence.
- After 1.0.0, use patch increments for compatible fixes, minor increments for compatible features, and major increments for breaking compatibility changes. Define compatibility around the supported CLI, configuration, and saved template behavior; record intentional breaks and migration guidance in release notes.

## Architecture and migration approach

Use .NET 10 LTS. Pin mutually compatible stable versions of Avalonia, its WebView, and the editor component during the initial technical prototype. Keep the existing WinForms source as a reference while adding a separate modern solution; there is no need to rename or reorganize every legacy file to begin.

Proposed layout:

```text
src/
  EpubMetadataEditor.Core/       C#: portable models and EPUB operations
  EpubMetadataEditor.Desktop/    C#: Avalonia views, view models, OS services
  EpubMetadataEditor.Cli/        C#: companion extraction commands
tests/                         shared core, CLI, and relevant UI tests
fixtures/                      small, redistributable EPUB test books
packaging/                     Windows, Linux, and macOS release definitions
```

The desktop and CLI projects depend on Core. Core has no dependency on either frontend, Windows Forms, browser controls, dialogs, clipboard, or process launching. Start with OS services inside Desktop rather than introducing a separate platform project before it is needed.

Separate the following responsibilities in Core: archive access and safe saving; package/metadata parsing and updates; cover changes; metadata transformations; filename templates and extraction; and batch execution. Return structured results and per-file errors instead of showing message boxes. Store metadata in models that retain repeated values, IDs, roles, refinements, and unknown XML, rather than treating the old form's textboxes as the data model.

Port useful algorithms from VB.NET to C# one feature at a time, with behavior tests. `OpenEPUB`, `ExtractMetadata`, and `SaveEpub` all live in `Form1.vb` and directly touch UI state, so their responsibilities must be separated during reimplementation. Preserving VB source in the new runtime is not an objective. A mechanical language conversion would retain these dependencies; use explicit models, portable services, and namespace-aware XML editing while preserving intended behavior and reader compatibility.

Use namespace-aware XML APIs and evaluate `System.IO.Compression.ZipArchive` against the existing ZIP behavior before replacing `zipstorer.vb`. Keep EPUB archive paths and URLs separate from operating-system filesystem paths. Avoid changes to the process-wide working directory. Image rendering and manipulation must use portable Avalonia/Skia APIs rather than `System.Drawing` image classes.

## Feature parity inventory

These are the audited feature groups. Milestone 0 expands each group into individual user actions, examples, expected results, and Windows/Linux/macOS verification columns. Each row starts unimplemented; a successful build is not proof of feature parity.

| Feature group | Required behavior | Existing reference |
| --- | --- | --- |
| Metadata editing | Title and file-as, creators and roles/file-as, publisher, description, publication date, type, format, identifiers, source, language, subjects, series/index; preserve repeated and unexposed metadata | `Form1.vb`, especially `ExtractMetadata` and `SaveEpub` |
| Metadata helpers | Existing title-case, copy/swap, creator sort-name, and field editing helpers | `Form1.vb`, `Form7.vb`, `Form9.vb` |
| Covers | Preview, add/replace, select an existing image, clipboard and drag/drop import, export, SVG-related fixes, cover prioritization, thumbnails, OPF fixes, and apply-all behavior | `Form1.vb`, `Form10.vb` |
| Batch operations | File selection/drop, copy/clear/generate file-as values, title case, swap title/creator, sequential series numbering, cover fixes, template replacement, literal and regex find/replace; retain per-file failure information | `Button10_Click`, forms 7, 9, 10 |
| Rename | Individual/batch renaming, current template tokens, creator variants, subject/language, escaped percent signs, uppercase tokens, folder templates, preview, and collision/overwrite choices | `Form4.vb`, rename routines in `Form1.vb`, `Dialog4.vb` |
| Extract from filename | Supported template fields and subfolders, token interpretation, previews, and applying extracted metadata to selected files | `Form11.vb`, `Button46_Click` |
| Internal file editing | OPF, NCX, EPUB 3 navigation, page-map, and other text files; save/cancel, find, clipboard/undo, visible paragraph markers, formatting, metadata-tag removal with cover preservation, and sample guide insertion | `Form2.vb`, associated `Form1.vb` actions |
| Previews and TOC | Formatted description, HTML/XHTML preview with local images/styles, NCX tree, navigation preview, and external EPUB viewer including an unsaved preview snapshot | `Form1.vb`, forms 3, 5, 6 |
| Navigation and search | Folder book list, previous/next book, current-folder search/filter behavior, multiple metadata values, dirty-state prompts, keyboard shortcuts, and file drag/drop | `Form1.vb`, `Form8.vb` |
| Desktop integration | Remembered settings/window placement, configurable external viewer, show extracted contents in the file manager, open-book arguments/file association, About/credits and links, and update notification pointing to this fork's releases | `ClassIni.vb`, application startup, `Dialog3.vb`, installer |
| Companion console | Inventory and reproduce metadata-field and cover extraction commands through the shared Core | Bundled `EPubMetadataEditorConsole.exe`; installer uses `-extract cover` |

The repository contains the console executable but no corresponding source project. Static inspection shows usage of the form `EPubMetadataEditorConsole.exe "filename" -extract fieldname`. Static inspection has now recorded its advertised fields in docs/MILESTONE_0.md; their runtime behavior, output formats, and exit codes still need verification in Milestone 0. Include this in parity planning rather than assuming the executable is portable or that the known cover command is its only feature.

Parity means users can complete the same workflows with equivalent results. Adapt labels such as “Windows Explorer,” native dialogs, menus, and Ctrl/Command shortcuts to each OS. Preserve behavior deliberately; do not reproduce data-loss bugs or silently drop a feature because its Windows control has no direct replacement.

## Milestones and completion criteria

| Milestone | Work | Complete when |
| --- | --- | --- |
| 0. Establish baseline and prove dependencies | Expand the feature matrix; collect sample inputs/outputs; install/pin the toolchain; create the modern solution; prototype a real metadata operation and a filename-template operation in C#, plus file dialogs, image handling, raw text editing, and embedded HTML previews; establish three-OS CI | Shared Core and a small Avalonia prototype build on all three OSes; runtime checks establish the component choices; console behavior/source gap and supported platform versions are recorded |
| 1. Implement portable EPUB core | Port and refactor metadata logic in C#, resolve the package through `META-INF/container.xml`, preserve namespaces and metadata relationships, support EPUB 2/3 covers, implement safe saving and reusable transformations | Open/edit/save/reopen fixtures pass on all three OSes; unrelated archive contents are preserved; injected save failures leave the original intact |
| 2. Implement complete single-book workflow | Metadata UI and helpers, covers and fixes, folder navigation/search, settings, open/save/save-as behavior, unsaved changes, external viewer, file manager, keyboard and drag/drop behavior | Every single-book parity action passes; the UI remains responsive during file operations |
| 3. Implement batch and filename tools | All batch transformations, template expansion, regex replacement, series sequencing, rename collision handling, extraction from filenames, and companion CLI | Golden examples match intended legacy results; partial failures and cancellation are reported without corrupting other books; CLI behavior is verified |
| 4. Implement advanced editing and previews | Internal file editor and helpers, TOC tree, navigation/HTML previews, description rendering, and synchronization between raw file edits and metadata controls | Edited content and local preview resources work on all target platforms; raw edits cannot be silently overwritten by stale form state |
| 5. Validate and package full-parity release | Finish every parity row; run reader checks; configure update links; bundle dependencies, launcher/file associations, documentation, and release packages | All required parity rows pass on Windows/Linux/macOS, EPUB regression checks pass, and packaged builds run on clean test systems |

Preview and editor integration prototypes happen in Milestone 0 even though the complete advanced workflow is delivered in Milestone 4. Do not leave feasibility discovery until the end. Estimate calendar dates after the baseline and prototypes establish the actual workload.

## Data preservation and regression tests

Saving is a release-critical part of this port. The current implementation deletes the original EPUB before copying a temporary replacement (`Form1.vb:4738`). The replacement implementation must write a unique temporary file beside the destination, finish and close it, check its structure, and replace the destination using suitable same-filesystem operations. Preserve a recoverable original until success, report permission/lock failures, and detect a source file changed by another application. Cancellation and error cleanup must never discard the only complete copy. Document the supported filesystem behavior rather than promising crash safety on every remote filesystem.

For metadata-only edits, compare the decompressed bytes of unrelated archive entries before and after saving. Preserve unknown metadata, repeated creators/identifiers, namespace relationships, EPUB version, manifest/spine order, and unchanged resources. Handle EPUB 2 attributes and EPUB 3 refinements/series metadata separately. Changing the publication identifier can affect obfuscated fonts; cover that case explicitly before enabling such a change for affected books.

Ensure `mimetype` is the first ZIP entry, stored without compression or a local-header extra field, with the exact required content. Resolve archive entry names without allowing extraction outside the working directory. Treat malformed packages, unsupported encryption, and ambiguous or unsafe modifications as explicit errors with the source preserved.

Build a small fixture set covering EPUB 2/3, namespace variations, nested/encoded URLs, non-ASCII names/text, repeated fields, PNG/JPEG/SVG covers, missing covers, font obfuscation, malformed XML, corrupt ZIPs, uppercase extensions, case-sensitive paths, and rename conflicts. Add targeted cases for the Kobo regression mentioned in the current changelog if a redistributable reproducer is available.

Run core and CLI tests on all three OSes. Use EPUBCheck on valid fixtures and compare before/after diagnostics for deliberately invalid ones. Test faults during write, validation, and replacement. Add focused UI integration tests for dirty-state prompts, batch results, and raw-editor synchronization, plus real-desktop checks for dialogs, clipboard, drag/drop, DPI, and previews. Finish with reader smoke tests; EPUBCheck alone does not establish Kobo or other reader compatibility.

## Platform and distribution plan

Proposed initial test targets are Windows 11 x64, Linux x64 on a maintained Ubuntu release plus the maintainer's CachyOS environment, and macOS on Apple Silicon. Add Intel macOS and additional ARM64 targets only with runtime test coverage. Record exact minimum OS/library versions after pinning dependencies. This is an intended support matrix, not a claim of verified support today.

On Linux, explicitly test X11 and a Wayland desktop and record whether XWayland is required. Avalonia currently defaults to X11 and offers an opt-in Wayland backend; native Wayland is a separate validation decision from running natively on Linux. CachyOS/Arch is outside Avalonia's main supported-distribution tiers, so it needs its own application tests.

Prefer Avalonia's native WebView for the existing previews. Its current documentation describes Windows WebView2, macOS WKWebView, and Linux WebKitGTK/WPE dependencies. Prototype the chosen released package with local XHTML, images, relative stylesheets, and the actual Linux display configuration. Use the standard Avalonia package documentation, not the separate XPF/WPF compatibility documentation. An external browser may be a troubleshooting fallback; it must not silently replace a required embedded-preview workflow.

Use AvaloniaEdit as the initial raw-text-editor candidate and validate compatibility with the selected Avalonia release. Preserve the existing editor actions before adding unrelated editor features. Store settings in per-user OS-appropriate locations; desktop services handle file associations, launching URLs/files, and user-visible shortcuts.

Publish self-contained builds so users do not need a separately installed .NET runtime. Native graphics/browser dependencies still need installation or packaging. Proposed public formats are a Linux AppImage, Windows installer, and macOS app bundle/DMG; prove one format per OS rather than promising every package manager. Build packaging jobs on appropriate OS runners and plan macOS signing/notarization and Windows signing credentials before public distribution. Flatpak and distro-specific packages can be evaluated after the first release unless needed to deliver the agreed Linux behavior.

Keep upstream attribution and the existing GPL-3.0-or-later notice, and include dependency notices in release artifacts. Add appropriate ignore/build configuration for the new solution; do not remove tracked legacy binaries or signing artifacts merely as incidental cleanup while establishing the baseline.

## Milestone 0 progress

Milestone 0 is in progress. The separate C# solution, desktop shell, development CLI, regression tests, generated fixtures, parity checklist, and three-OS CI workflow have been added. Title-copy saving and a subset of filename-template expansion are implemented. The native component probe loads a text editor, raster image, and local HTML with relative resources. See [docs/MILESTONE_0.md](docs/MILESTONE_0.md) for verification evidence and remaining completion criteria; [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) contains build and run commands.

The local .NET 10.0.112 SDK has been installed and verified. Linux builds and prototype tests have run; legacy runtime comparisons and Windows/macOS verification are still pending. The maintainer has Windows access; macOS desktop testing still needs an arrangement. Distribution-signing credentials are prerequisites to completing the later release checks.

## References

- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy): .NET 10 LTS.
- [Avalonia supported platforms](https://docs.avaloniaui.net/docs/supported-platforms): platform tiers and desktop prerequisites.
- [Avalonia embedding web content](https://docs.avaloniaui.net/docs/app-development/embedding-web-content): preview backends and native dependencies.
- [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit): raw text editor candidate.
- [EPUB 3.3](https://www.w3.org/TR/epub-33/): container, metadata, ZIP and font-obfuscation requirements; preserve existing EPUB 2 semantics too.
- [EPUBCheck](https://www.w3.org/publishing/epubcheck/): validation for EPUB 2 and EPUB 3.
- [.NET deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/): self-contained publishing.
