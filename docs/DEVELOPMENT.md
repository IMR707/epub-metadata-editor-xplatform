# Development guide

The new solution is `EpubMetadataEditor.XPlatform.slnx`. The original `EPubMetadataEditor.sln` and `epubmetadataeditor/` remain the Windows reference implementation.

## Toolchain

`global.json` selects SDK 10.0.112 and permits servicing updates within the same SDK feature band. `Directory.Packages.props` pins direct packages; checked-in `packages.lock.json` files pin their resolved dependency graphs. Update these intentionally together.

On CachyOS/Arch:

```sh
sudo pacman -Syu --needed dotnet-sdk-10.0
```

Native Linux UI dependencies include X11/XWayland, fontconfig, and graphics libraries. The embedded preview uses WebKitGTK when WPE is unavailable. The current CachyOS test machine already has GTK 3, WebKitGTK 4.1 and libsoup 3. If these are missing on another machine:

```sh
sudo pacman -Syu --needed gtk3 webkit2gtk-4.1 libsoup3
```

Do not install MAUI workloads for this Avalonia project. The SDK's workload-verification advisory did not prevent this prototype from building. A normal NuGet restore can download missing targeting packs (including ASP.NET reference packs requested by the distribution SDK).

## Build, test, and run

Run from the repository root:

```sh
dotnet restore EpubMetadataEditor.XPlatform.slnx --locked-mode
dotnet build EpubMetadataEditor.XPlatform.slnx --configuration Release --no-restore
dotnet test EpubMetadataEditor.XPlatform.slnx --configuration Release --no-build --no-restore
dotnet run --project src/EpubMetadataEditor.Desktop --configuration Release --no-build
```

Open `fixtures/epub3-unicode.epub`, change its title, and use **Save a copy** with a new filename. Reopen the output to verify the title. Existing destinations, including the source file, are refused; this prototype intentionally has no in-place save. Closing or opening another book prompts when the current title is edited. A saved copy does not replace the currently open original.

The filename preview supports `%Title%`, `%Creator%`, `%Creator2%`, `%Language%`, their uppercase variants, and `%%`. It does not rename files. It uses invariant uppercase, rejects unsupported/malformed tokens, sanitizes portable filename characters, and protects Windows reserved basenames. These are documented decisions, not a claim that every legacy template is supported.

## Component feasibility probe

Use the **Component checks** button or:

```sh
dotnet run --project src/EpubMetadataEditor.Desktop --configuration Release --no-build -- --component-probe
```

The window contains an editable AvaloniaEdit document, a generated raster cover, a native file-dialog button, and a NativeWebView with a bundled local page. The page uses relative CSS and image resources. The browser executes a DOM check for those resources and reports success or failure. Only trusted bundled sample content is loaded; arbitrary EPUB HTML rendering belongs to Milestone 4.

For an unattended native-browser check on an actual desktop session (not a headless CI runner):

```sh
dotnet run --project src/EpubMetadataEditor.Desktop --configuration Release --no-build -- --component-probe --auto-check
```

It exits with 0 after the browser resource check passes and 1 on failure/timeout. Run the interactive probe separately to check the file dialog and editor keyboard behavior. A successful headless test run cannot establish native-browser or file-dialog compatibility.

## Development CLI

```sh
dotnet run --project src/EpubMetadataEditor.Cli -- inspect fixtures/epub3-unicode.epub
dotnet run --project src/EpubMetadataEditor.Cli -- set-title fixtures/epub3-unicode.epub /tmp/edited-book.epub "New title"
dotnet run --project src/EpubMetadataEditor.Cli -- preview-name fixtures/epub3-unicode.epub "%Creator% - %TITLE%.epub"
```

Use an appropriate new output path on Windows. `inspect` emits JSON. Exit codes are 0 for success/help, 1 for a book/file operation failure, and 2 for invalid arguments. These are prototype commands; the bundled legacy console's `-extract` interface still needs porting.

## Test materials

Fixtures and their generator live in `fixtures/` and `scripts/generate-fixtures.py`. Tests compare decompressed archive entries, unedited metadata XML, source bytes, and the ZIP mimetype header. They also cover failed/cancelled operations, malformed XML, unsafe archive names, CLI failures, and view-model bindings.

The current memory-based prototype supports one package rootfile, up to 4096 entries, at most 32 MiB per entry and 128 MiB total uncompressed. Larger and multi-rendition books receive explicit errors. Streaming and broader format handling are future core work.

Use [EPUBCheck](https://www.w3.org/publishing/epubcheck/) for conformance validation:

```sh
java -jar /path/to/epubcheck.jar fixtures/epub2-nested.epub
java -jar /path/to/epubcheck.jar fixtures/epub3-unicode.epub
```

The space in each sample chapter filename intentionally exercises URL encoding and produces PKG-010. Other diagnostics require investigation. EPUBCheck does not replace actual reader tests.

## Architecture map

- **Core**: models, archive/package handling, title-copy saving, and filename expansion. No UI dependency.
- **Desktop**: Avalonia XAML views, a view model, file-dialog/window services, and the component probe.
- **CLI**: command parsing and output around the same Core operations.
- **Tests**: Core/CLI regressions and Avalonia headless binding tests.

For a Java/PHP/TypeScript developer: `.csproj` is the project/build manifest, NuGet provides dependencies, `.axaml` defines the UI, and the view model holds observable UI state. EPUB operations belong in Core rather than window event handlers.

## Verified versions and reference documentation

- .NET SDK 10.0.112; local runtime 10.0.12.
- Avalonia/Desktop/Fluent/Headless 12.1.3.
- Avalonia.Controls.WebView 12.1.0; Avalonia.AvaloniaEdit 12.0.0.
- CommunityToolkit.Mvvm 8.4.2; xUnit v3 3.2.2; VSTest package 18.10.1.
- [Avalonia embedded web content](https://docs.avaloniaui.net/docs/app-development/embedding-web-content).
- [Avalonia supported platforms](https://docs.avaloniaui.net/docs/supported-platforms).

Runtime minimum OS/library versions and release packaging are not established by these dependency pins. See the milestone tracker for actual verification status.
