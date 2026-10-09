# EPUB Metadata Editor XPlatform

A project to reimplement [Ben Chenoweth's EPUB Metadata Editor](https://github.com/benchen71/epub-metadata-editor) in **C# with .NET and Avalonia**, targeting **Windows, Linux, and macOS**.

This will be a new implementation with its own architecture and release schedule. The repository remains a fork to preserve its connection to the original project, source, and history. The planned application will share its core and desktop interface across all three platforms.

## Current status

**Milestone 0 in progress — an initial C#/Avalonia development prototype is available.**

The repository preserves the original **VB.NET / Windows Forms application, version 2.0.4**, targeting .NET Framework 3.5, alongside the new C# solution. Executables in the legacy source tree are Windows builds of the original application.

The prototype opens EPUB 2/3 books, displays metadata and raster covers, edits the first title, saves a new copy, and previews a small subset of filename-template fields. A component probe exercises a text editor and embedded HTML rendering. This is not a full-parity release. Windows and macOS verification remains pending.

For the existing Windows application, see the [original project's releases](https://github.com/benchen71/epub-metadata-editor/releases) and [documentation](https://github.com/benchen71/epub-metadata-editor/wiki).

## Run the development prototype

Install the .NET 10 SDK, then run from the repository root:

```sh
dotnet run --project src/EpubMetadataEditor.Desktop
```

Use one of the generated books in `fixtures/` to try the title-edit/save-copy workflow. See [DEVELOPMENT.md](docs/DEVELOPMENT.md) for native dependencies, build/test commands, the CLI, and component checks. Track actual verification in [MILESTONE_0.md](docs/MILESTONE_0.md) and the [feature parity checklist](docs/FEATURE_PARITY.md).

## Planned functionality

The first cross-platform release is intended to reach **full feature parity** with the original application. The list below describes the complete release target; only the prototype subset described above is implemented:

- **EPUB 2 and EPUB 3 metadata editing:** titles, creators and roles, sort fields, publisher, description, dates, identifiers, language, subjects, and series information.
- **Cover tools:** preview, import, replace, select existing images, clipboard and drag/drop support, export, thumbnails, and cover fixes.
- **Batch operations:** metadata transformations, field replacement, literal and regular-expression find/replace, and series numbering.
- **Filename tools:** individual and batch renaming, filename templates, conflict handling, and metadata extraction from filenames.
- **Internal file editing:** OPF, NCX, navigation documents, page maps, and other text files, with the existing editing helpers.
- **Previews and navigation:** table of contents, HTML/XHTML and description previews, folder browsing/search, and external viewer integration.
- **Desktop and command-line integration:** settings, shortcuts, file opening, and metadata/cover extraction commands.

Intermediate development milestones will establish and test these features before the full-parity release.

## Implementation direction

The new application will use **C# throughout its core, desktop interface, command-line tools, and tests**, with **.NET 10** and **Avalonia** as the planned foundation.

The original VB.NET implementation will serve as a reference for workflows, EPUB conventions, and reader compatibility. Useful algorithms will be ported selectively to C# and covered by regression tests. The new design will separate EPUB processing from UI controls and replace Windows-specific dependencies.

Development priorities include:

- Preserving unrelated book contents and metadata when saving.
- Writing and validating replacement files before replacing originals.
- Handling XML namespaces, EPUB archive paths, and operating-system paths correctly.
- Testing feature parity and packaged applications on all three target platforms.

See [CROSS_PLATFORM_PLAN.md](CROSS_PLATFORM_PLAN.md) for the feature inventory, architecture, milestones, and release criteria.

## Development and versioning

Port development takes place on `port/cross-platform`. The `master` branch remains the latest stable release; it currently contains the original Windows baseline. Once the cross-platform application meets the full-parity release criteria, its release-ready code will be merged into `master`.

XPlatform uses its own release numbers, independent of the original application's 2.0.4 baseline. Internal development milestones use `0.x.y`; the first full-parity public release is planned as `1.0.0`. Release tags use the prefix `xplatform-v`, for example `xplatform-v1.0.0`, while existing upstream tags and history are preserved. These decisions do not change the GitHub fork relationship.

## Repository layout

- [`epubmetadataeditor/`](epubmetadataeditor/) — original VB.NET application, resources, and legacy build artifacts.
- [`EPubMetadataEditor.sln`](EPubMetadataEditor.sln) — original Windows solution.
- [`CROSS_PLATFORM_PLAN.md`](CROSS_PLATFORM_PLAN.md) — cross-platform implementation plan.

- [`EpubMetadataEditor.XPlatform.slnx`](EpubMetadataEditor.XPlatform.slnx) — new .NET 10 solution.
- [`src/`](src/) — shared Core, Avalonia Desktop, and development CLI.
- [`tests/`](tests/) and [`fixtures/`](fixtures/) — regression tests and original EPUB samples.
- [`docs/`](docs/) — developer guide, milestone evidence, and feature checklist.

## Credits and license

Original EPUB Metadata Editor by **Ben Chenoweth**.

Copyright (C) 2011-2026 Ben Chenoweth.

This cross-platform reimplementation is developed with assistance from OpenAI Codex.

This fork is maintained under [IMR707/epub-metadata-editor-xplatform](https://github.com/IMR707/epub-metadata-editor-xplatform). Original author and third-party notices are retained in the source.

This program is free software: you can redistribute it and/or modify it under the terms of the **GNU General Public License** as published by the Free Software Foundation, either **version 3** of the License, or **(at your option) any later version**.

This program is distributed in the hope that it will be useful, but **WITHOUT ANY WARRANTY**; without even the implied warranty of **MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE**. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see the [GNU General Public License](https://www.gnu.org/licenses/gpl-3.0.html).
