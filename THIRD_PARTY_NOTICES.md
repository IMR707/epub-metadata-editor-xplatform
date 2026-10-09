# Attribution and dependencies

EPUB Metadata Editor XPlatform is a C# reimplementation derived from Ben Chenoweth's EPUB Metadata Editor, copyright (C) 2011-2026 Ben Chenoweth. The application remains GPL-3.0-or-later; the existing license notice is preserved in `epubmetadataeditor/bin/Release/EPubMetadataEditor License.txt`. Original source comments and third-party notices remain in the legacy tree. Development uses OpenAI Codex assistance.

The initial filename expansion behavior is adapted from `epubmetadataeditor/Form4.vb`. The new archive/XML code uses .NET APIs; it does not copy the legacy ZipStorer implementation.

Direct production dependencies currently include .NET, Avalonia, AvaloniaEdit, Avalonia.Controls.WebView, and CommunityToolkit.Mvvm, whose package metadata declares MIT licenses. Tests additionally use xUnit, Avalonia.Headless.XUnit, Microsoft.NET.Test.Sdk, and the Visual Studio xUnit adapter. Exact versions and transitive dependencies are recorded in `Directory.Packages.props` and project `packages.lock.json` files.

This development inventory is not the complete notice bundle for distributed binaries. Release packaging must collect applicable notices for all shipped managed/native dependencies, including graphics/browser components, before release. No upstream third-party license notices are removed by this migration.

Original synthetic fixture content and its generated cover are dedicated under CC0-1.0; see `fixtures/README.md`.
