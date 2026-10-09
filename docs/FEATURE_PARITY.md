# Feature parity checklist

Each row is a user workflow to verify against the original Windows app. **Pending** means no parity claim. The title/save-copy and four-token prototypes have automated coverage, but their broader workflow rows stay pending until the full behavior and real desktop checks pass. The component probe does not establish complete book-preview/editor parity.

Examples below are test targets, not recorded legacy runtime results. The original app has not yet been executed for comparison.

| ID | User action | Expected example/result | Legacy reference | Linux | Windows | macOS |
| --- | --- | --- | --- | --- | --- | --- |
| META-01 | Edit title | Change first title; preserve secondary titles and refinements | Form1.ExtractMetadata / SaveEpub | Pending | Pending | Pending |
| META-02 | Edit title sort value | TitleFileAs round trip without changing displayed title | Form1 / TextBox16 | Pending | Pending | Pending |
| META-03 | Edit primary creator | Name, role and file-as retain relationships | Form1 / TextBox2, TextBox12 | Pending | Pending | Pending |
| META-04 | Edit additional creators | Preserve multiple creators while editing first/second | Form1 / TextBox3, TextBox13 | Pending | Pending | Pending |
| META-05 | Edit publisher and description | Preserve Unicode and escaped markup | Form1 | Pending | Pending | Pending |
| META-06 | Edit date and language | Retain supplied date precision and language value | Form1 | Pending | Pending | Pending |
| META-07 | Edit identifiers/source/type/format | Retain unknown identifiers; assess obfuscated fonts | Form1 | Pending | Pending | Pending |
| META-08 | Edit subjects | Preserve repeated subjects and Unicode | Form1 / TextBox17 | Pending | Pending | Pending |
| META-09 | Edit series/index | EPUB 2 Calibre fields and EPUB 3 relationships | Form1 / TextBox14, TextBox15 | Pending | Pending | Pending |
| META-10 | Metadata helpers | Title case, copy/swap, creator sort-name generation | Form1 | Pending | Pending | Pending |
| META-11 | Preserve unexposed metadata | Unknown properties, IDs, roles, refinements and comments survive saving | Form1 / SaveEpub | Pending | Pending | Pending |
| FILE-01 | Open EPUB via container | Nested OPF location; EPUB 2 and 3; encoded references | Form1.OpenEPUB | Pending | Pending | Pending |
| FILE-02 | Save existing book | Safe replacement with recoverable source on failure | Form1.SaveEpub | Pending | Pending | Pending |
| FILE-03 | Save as/copy | New destination; handle collisions and failed writes | Form1.SaveEpub | Pending | Pending | Pending |
| FILE-04 | Unsaved changes | Open/close/cancel prompts protect edits | Form1 / Dialog1 | Pending | Pending | Pending |
| FILE-05 | Folder navigation | List books and move previous/next | Form1 | Pending | Pending | Pending |
| FILE-06 | Search/filter folder | Match existing folder search behavior | Form8 | Pending | Pending | Pending |
| FILE-07 | Open arguments and drag/drop | Open books from shell arguments and desktop drops | Form1 startup and drop handlers | Pending | Pending | Pending |
| COVER-01 | Preview covers | EPUB 2/3 cover declarations; PNG/JPEG/SVG | Form1 | Pending | Pending | Pending |
| COVER-02 | Replace/add cover | Update manifest and cover relationships correctly | Form1 | Pending | Pending | Pending |
| COVER-03 | Select existing image | Choose an archive resource as cover | Form1 | Pending | Pending | Pending |
| COVER-04 | Clipboard/drop import | Import supported image data and files | Form1 | Pending | Pending | Pending |
| COVER-05 | Export cover | Preserve expected format and report conflicts | Form1 | Pending | Pending | Pending |
| COVER-06 | Cover repair options | SVG fixes, prioritization, thumbnails and OPF fixes | Form1 / Form10 | Pending | Pending | Pending |
| BATCH-01 | Select and process files | Multiple selection/drop with per-book results | Form1.Button10_Click | Pending | Pending | Pending |
| BATCH-02 | Copy/clear/generate fields | File-as helpers and title/creator swaps | Form1.Button10_Click | Pending | Pending | Pending |
| BATCH-03 | Title case | Supported casing helpers across chosen fields | Form1.TitleCase | Pending | Pending | Pending |
| BATCH-04 | Series sequencing | Apply start/index sequence deterministically | Form1.Button10_Click | Pending | Pending | Pending |
| BATCH-05 | Replacement templates | Replace fields using supported metadata tokens | Form7 / Form1.ReplaceField | Pending | Pending | Pending |
| BATCH-06 | Find and replace | Literal and regex modes, errors and empty matches | Form9 | Pending | Pending | Pending |
| BATCH-07 | Apply cover fixes | Apply selected options to all chosen books | Form10 | Pending | Pending | Pending |
| BATCH-08 | Partial failure and cancellation | Report failures without corrupting other books | Form1.Button10_Click | Pending | Pending | Pending |
| NAME-01 | Basic template fields | Creator/Creator2/Title/Language and uppercase variants | Form4.UpdateFilename | Pending | Pending | Pending |
| NAME-02 | Extended template fields | File-as, surnames/initials, date, series/index and subject | Form4.UpdateFilename | Pending | Pending | Pending |
| NAME-03 | Literal percent and preview | %%; percent parsing and filename sanitization | Form4.UpdateFilename | Pending | Pending | Pending |
| NAME-04 | Folder templates | Portable subfolder paths and segment trimming | Form4 / Form1 rename routines | Pending | Pending | Pending |
| NAME-05 | Rename selected books | Individual/batch apply with collision choices | Form1 / Dialog4 | Pending | Pending | Pending |
| NAME-06 | Extract metadata from filename | Fields, subfolders, token interpretation and preview | Form11 / Form1.Button46_Click | Pending | Pending | Pending |
| EDIT-01 | Edit OPF and NCX | Open/save/cancel raw text without losing metadata edits | Form2 | Pending | Pending | Pending |
| EDIT-02 | Edit navigation/page-map/other text | Load corresponding internal resources | Form2 / Form1 | Pending | Pending | Pending |
| EDIT-03 | Editor commands | Find, undo, clipboard and paragraph markers | Form2 | Pending | Pending | Pending |
| EDIT-04 | Formatting and helpers | Format XML, remove metadata tags, preserve cover, insert guide | Form2 | Pending | Pending | Pending |
| EDIT-05 | Raw/UI synchronization | Prevent stale UI state from overwriting raw edits | Form1 / Form2 | Pending | Pending | Pending |
| PREVIEW-01 | Description preview | Render formatted description | Form1 | Pending | Pending | Pending |
| PREVIEW-02 | HTML/XHTML preview | Local CSS/images, relative URLs and navigation | Form5 / Form6 | Pending | Pending | Pending |
| PREVIEW-03 | TOC tree | Display/edit NCX structure and labels | Form3 / WindowsVistaTreeView | Pending | Pending | Pending |
| PREVIEW-04 | EPUB 3 navigation preview | Load navigation document from manifest | Form1 / Form5 | Pending | Pending | Pending |
| PREVIEW-05 | External reader | Configured viewer and unsaved preview snapshot | Form1 | Pending | Pending | Pending |
| OS-01 | Settings persistence | Remember preferences and valid window placement | ClassIni / Form1 startup | Pending | Pending | Pending |
| OS-02 | File manager integration | Show extracted contents using OS file manager | Form1 | Pending | Pending | Pending |
| OS-03 | Keyboard and dialogs | OS-appropriate shortcuts, picker/collision prompts | Forms / Dialog1 / Dialog4 | Pending | Pending | Pending |
| OS-04 | Updates | Check this fork release channel and correct platform package | Dialog3 / Form1 | Pending | Pending | Pending |
| OS-05 | About and attribution | Original author, GPL and Codex assistance note | AboutBox1 / README | Pending | Pending | Pending |
| OS-06 | Associations and packaging | Install/open on clean supported OSes | Setup Script.iss | Pending | Pending | Pending |
| CLI-01 | Extract metadata fields | 14 advertised fields; verify output/encoding and missing values | Bundled EPubMetadataEditorConsole.exe | Pending | Pending | Pending |
| CLI-02 | Extract cover | File location, naming, format, stdout and exit status | Console executable / installer | Pending | Pending | Pending |
| CLI-03 | Errors and compatibility | Missing/corrupt files, invalid arguments and exit codes | Console executable | Pending | Pending | Pending |

Record verification evidence (test, fixture, OS, date and outcome) when changing a cell to Passed. All required rows must pass before 1.0.0.
