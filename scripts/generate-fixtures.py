#!/usr/bin/env python3
"""Generate original, redistributable EPUB fixtures using only Python's standard library."""
from pathlib import Path
import base64
import hashlib
import json
import struct
import zipfile
import zlib

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'fixtures'

def png():
    width, height = 120, 160
    rows = bytearray()
    for y in range(height):
        rows.append(0)
        for x in range(width):
            color = (22, 70, 80) if x < 10 else (35, 109, 117)
            if 22 < x < 105 and (35 < y < 43 or 54 < y < 60): color = (230, 233, 204)
            rows.extend(color)
    def chunk(kind, data):
        return struct.pack('>I', len(data)) + kind + data + struct.pack('>I', zlib.crc32(kind + data))
    return b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', width, height, 8, 2, 0, 0, 0)) + chunk(b'IDAT', zlib.compress(bytes(rows))) + chunk(b'IEND', b'')

def write_epub(name, version):
    folder = f'Books/EPUB{version}/'
    metadata_extra = ('<meta name="cover" content="cover"/><meta name="calibre:series" content="Fixture Series"/>' if version == 2 else
        '<meta property="dcterms:modified">2026-10-09T00:00:00Z</meta><meta property="demo:note">Preserve this custom metadata</meta><meta refines="#title" property="title-type">main</meta>')
    nav_item = '' if version == 2 else '<item id="nav" href="nav.xhtml" media-type="application/xhtml+xml" properties="nav"/>'
    cover_property = '' if version == 2 else ' properties="cover-image"'
    opf = f'''<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://www.idpf.org/2007/opf" xmlns:dc="http://purl.org/dc/elements/1.1/" version="{version}.0" unique-identifier="book-id"{(' prefix="demo: https://example.invalid/vocab#"' if version == 3 else '')}>
  <metadata>
    <!-- Keep this comment and the secondary title. -->
    <dc:identifier id="book-id">urn:uuid:67d010dc-56e5-413e-879a-236f25b3160{version}</dc:identifier>
    <dc:title id="title">The Sample Book</dc:title><dc:title>Secondary title</dc:title>
    <dc:creator>Zoë Chen</dc:creator><dc:creator>陈 Example</dc:creator>
    <dc:language>en</dc:language><dc:publisher>XPlatform fixtures</dc:publisher>
    {metadata_extra}
  </metadata>
  <manifest>
    <item id="chapter" href="Text/chapter%20one.xhtml" media-type="application/xhtml+xml"/>
    <item id="cover" href="Images/cover.png" media-type="image/png"{cover_property}/>
    <item id="style" href="Styles/book.css" media-type="text/css"/>
    <item id="ncx" href="toc.ncx" media-type="application/x-dtbncx+xml"/>{nav_item}
  </manifest>
  <spine toc="ncx"><itemref idref="chapter"/></spine>
</package>
'''
    container = f'<?xml version="1.0"?><container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container"><rootfiles><rootfile full-path="{folder}package.opf" media-type="application/oebps-package+xml"/></rootfiles></container>'
    chapter = '<?xml version="1.0" encoding="utf-8"?><html xmlns="http://www.w3.org/1999/xhtml"><head><title>Fixture chapter</title><link rel="stylesheet" type="text/css" href="../Styles/book.css"/></head><body><h1>Fixture chapter</h1><p>Original test content. Zoë · 陈</p><p><img src="../Images/cover.png" alt="Generated book cover"/></p></body></html>'
    ncx = f'<?xml version="1.0"?><ncx xmlns="http://www.daisy.org/z3986/2005/ncx/" version="2005-1"><head><meta name="dtb:uid" content="urn:uuid:67d010dc-56e5-413e-879a-236f25b3160{version}"/><meta name="dtb:depth" content="1"/><meta name="dtb:totalPageCount" content="0"/><meta name="dtb:maxPageNumber" content="0"/></head><docTitle><text>The Sample Book</text></docTitle><navMap><navPoint id="chapter" playOrder="1"><navLabel><text>Fixture chapter</text></navLabel><content src="Text/chapter%20one.xhtml"/></navPoint></navMap></ncx>'
    entries = {'mimetype': b'application/epub+zip', 'META-INF/container.xml': container.encode(), folder + 'package.opf': opf.encode(), folder + 'Text/chapter one.xhtml': chapter.encode(), folder + 'Images/cover.png': png(), folder + 'Styles/book.css': b'body { color: #163a45; }', folder + 'toc.ncx': ncx.encode()}
    if version == 3:
        entries[folder + 'nav.xhtml'] = b'<?xml version="1.0"?><html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops"><head><title>Contents</title></head><body><nav epub:type="toc"><h1>Contents</h1><ol><li><a href="Text/chapter%20one.xhtml">Fixture chapter</a></li></ol></nav></body></html>'
    path = OUT / name
    with zipfile.ZipFile(path, 'w') as archive:
        for name, data in entries.items():
            info = zipfile.ZipInfo(name, (2026, 1, 1, 0, 0, 0))
            info.compress_type = zipfile.ZIP_STORED if name == 'mimetype' else zipfile.ZIP_DEFLATED
            archive.writestr(info, data)
    return {'file': path.name, 'sha256': hashlib.sha256(path.read_bytes()).hexdigest(), 'epubVersion': f'{version}.0', 'title': 'The Sample Book', 'creators': ['Zoë Chen', '陈 Example'], 'packagePath': folder + 'package.opf', 'license': 'CC0-1.0', 'purpose': 'Nested package path, repeated creators/titles, Unicode, PNG cover and encoded resource URLs'}

OUT.mkdir(exist_ok=True)
manifest = [write_epub('epub2-nested.epub', 2), write_epub('epub3-unicode.epub', 3)]
(OUT / 'manifest.json').write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + '\n')
preview = ROOT / 'src/EpubMetadataEditor.Desktop/Assets/Preview/cover.png'
preview.parent.mkdir(parents=True, exist_ok=True)
preview.write_bytes(png())
print('Generated two EPUB fixtures and an original preview cover.')
