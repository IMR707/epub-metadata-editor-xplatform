# EPUB fixtures

These tiny books and their cover artwork are original synthetic test materials, dedicated to the public domain under CC0-1.0. They contain no third-party book content. Run `python3 scripts/generate-fixtures.py` from the repository root to reproduce them. `manifest.json` records expected metadata and SHA-256 hashes.

The initial books cover EPUB 2/3, nested package paths, Unicode, repeated titles/creators, encoded resource URLs, EPUB 2/3 cover declarations, comments, and unexposed metadata. Tests derive missing-cover, duplicate-entry, unsafe-path, malformed XML, and cancellation cases in temporary directories.

EPUBCheck results and the remaining fixture requirements are tracked in `docs/MILESTONE_0.md`. SVG/JPEG covers, encrypted resources/font obfuscation, and the historical Kobo regression still need dedicated fixtures.
