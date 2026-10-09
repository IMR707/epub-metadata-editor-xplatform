# Milestone 0 — foundation and dependency prototypes

Status: **in progress**, started 2026-10-09. This is a development prototype, not a full-parity release.

## Delivered implementation

- [x] Separate .NET 10 solution with C# Core, Desktop, CLI, and test projects.
- [x] Pin SDK/direct package versions and transitive package lock files.
- [x] Add a feature-by-feature parity checklist and original EPUB 2/3 fixtures.
- [x] Implement container-based metadata reading and a title-edit/save-copy operation that preserves unrelated content.
- [x] Port a bounded filename-template operation with regression examples traced to Form4.
- [x] Build a desktop shell with open/save dialogs, title editing, raster cover display, and filename preview.
- [x] Integrate AvaloniaEdit and NativeWebView in a separate component probe.
- [x] Configure Windows/Linux/macOS build and test jobs.
- [x] Identify the legacy CLI's advertised extraction fields by static binary inspection.

## Verification record

| Check | CachyOS/Linux | Windows | macOS |
| --- | --- | --- | --- |
| Release build and automated Core/CLI/headless UI tests | Passed locally; see test results below | Pending CI execution | Pending CI execution |
| Native browser: local HTML, relative CSS and PNG image | Passed, GTK/WebKitGTK via XWayland on a Wayland session | Pending | Pending |
| Native image decoding and editor control construction | Passed in component probe | Pending | Pending |
| Open/save dialogs, title editing, save-copy and reopen | Passed; maintainer manual test, 2026-10-09 | Pending | Pending |
| Component-probe file picker and editor keyboard/clipboard interactions | Manual check pending | Pending | Pending |
| Main window loads fixture metadata and raster cover | Passed in native desktop smoke test | Pending | Pending |
| Legacy application behavior comparisons | No Windows runtime available locally | Maintainer has Windows access; comparison pending | Not applicable |

Headless tests validate bindings and view-model state, not a native desktop session. The CI workflow is configured but has not run remotely because these changes have not been pushed. Maintainer access covers CachyOS and Windows; macOS desktop testing still needs an arrangement. Native Wayland has not been enabled or verified; the Linux run used Avalonia's default X11 backend under XWayland.

## Open items before closing Milestone 0

- Run the configured CI jobs on all three OSes and resolve any platform failures.
- Run the component probe interactively, including dialogs and editor actions, on supported desktop systems.
- Capture original Windows UI and CLI outputs for golden comparisons; finish output-format, encoding, cover-path, and exit-code checks.
- Confirm practical minimum OS/library versions based on the selected components and runtime checks.
- Extend fixtures for missing/JPEG/SVG covers, font obfuscation and the historical Kobo issue as those core behaviors are introduced.

## Maintainer feedback — 2026-10-09

The maintainer confirmed that editing the title and saving a copy work on CachyOS. Screenshots show the edited EPUB reopened with its saved title and the unsaved-change confirmation displayed. This establishes the initial title-edit/save-copy/reopen workflow, not full metadata parity.

The confirmation dialog had excessive blank space below its buttons. Its fixed height has been replaced with content-based height, padding reduced, and actions aligned to the right. Other platform and component-probe manual checks remain open.

## Known prototype boundaries

Only the first title is editable. Other metadata is read/preserved, not exposed for editing. Saving always creates a new file and never overwrites an existing destination. Raw XML editing in the component probe is sample text; it is not connected to book saving. HTML previews currently use trusted bundled content. Batch editing, actual file renaming, full CLI compatibility, settings persistence, and installers are not implemented.

`Form4.UpdateFilename` is the legacy source for the initial filename examples. Unsupported tokens and unmatched percent signs produce explicit errors rather than silent deletion. Unicode uppercase uses the invariant culture for consistent output across OSes. Folder templates and the remaining legacy tokens are intentionally still pending.

## Legacy CLI source gap

The only console implementation in the repository is `epubmetadataeditor/bin/Release/EPubMetadataEditorConsole.exe`. Its embedded UTF-16 usage text advertises:

```text
EPubMetadataEditorConsole.exe "filename" -extract fieldname
Title, TitleFileAs, Creator, CreatorFileAs, CreatorRole,
SecondCreator, SecondCreatorFileAs, SecondCreatorRole,
Series, SeriesIndex, Publisher, Subject, Language, Cover
```

These field names come from static inspection, not execution. The installer also invokes `-extract cover`. A Windows runtime comparison must establish whitespace/encoding, missing-field behavior, cover naming/location and exit codes. Run legacy tools only against disposable fixture copies when capturing these results.

## Validation results

The initial suite contains 34 passing tests. The native main-window smoke test loaded the EPUB 3 fixture title and cover and captured the rendered window. Broader desktop workflow, accessibility and DPI checks remain pending. Both generated books and their title-edited copies passed EPUBCheck 5.4.0 with zero errors/fatals and the same one expected warning per book. The expected PKG-010 warning records the deliberate space in a resource filename; the acceptance criterion is no errors/fatals and no new warnings after editing.

See [development commands](DEVELOPMENT.md) and the [parity checklist](FEATURE_PARITY.md).
