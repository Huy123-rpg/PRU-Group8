// Restore UTF-8 bytes accidentally decoded as Windows-1252/Latin-1.
// Only reversible byte sequences are changed; valid Vietnamese stays intact.
import fs from 'node:fs';
import path from 'node:path';
import { TextDecoder } from 'node:util';
const cp = new Map();
const cpDecoder = new TextDecoder('windows-1252');
for (let i = 0; i < 256; i++) { cp.set(cpDecoder.decode(Uint8Array.of(i)), i); cp.set(String.fromCharCode(i), i); }
const extended = [0x20ac,0x81,0x201a,0x192,0x201e,0x2026,0x2020,0x2021,0x2c6,0x2030,0x160,0x2039,0x152,0x8d,0x17d,0x8f,
  0x90,0x2018,0x2019,0x201c,0x201d,0x2022,0x2013,0x2014,0x2dc,0x2122,0x161,0x203a,0x153,0x9d,0x17e,0x178];
extended.forEach((code, i) => cp.set(String.fromCodePoint(code), 0x80 + i));
const utf8 = new TextDecoder('utf-8', { fatal: true });
export function repair(input) {
  let s = input;
  for (let pass = 0; pass < 3; pass++) {
    let output = '';
    for (let i = 0; i < s.length;) {
      const lead = cp.get(s[i]);
      const n = lead >= 0xc2 && lead <= 0xdf ? 2 : lead >= 0xe0 && lead <= 0xef ? 3 : lead >= 0xf0 && lead <= 0xf4 ? 4 : 0;
      const bytes = Array.from(s.slice(i, i + n), c => cp.get(c));
      if (n && bytes.length === n && bytes.slice(1).every(b => b >= 0x80 && b <= 0xbf)) {
        try { output += utf8.decode(Uint8Array.from(bytes)); i += n; continue; } catch { /* not valid UTF-8 */ }
      }
      output += s[i++];
    }
    if (output === s) break;
    s = output;
  }
  return s;
}
const write = process.argv.includes('--write');
function walk(dir) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const file = path.join(dir, entry.name);
    if (entry.isDirectory()) { walk(file); continue; }
    if (!/\.(cs|json|csv|txt|md|unity|asset)$/.test(file)) continue;
    const buffer = fs.readFileSync(file);
    const utf16 = buffer[0] === 0xff && buffer[1] === 0xfe;
    const original = utf16 ? buffer.subarray(2).toString('utf16le') : buffer.toString('utf8');
    const fixed = repair(original);
    if (fixed === original && !utf16) continue;
    const editable = /\.(cs|json|csv|txt|md)$/.test(file);
    if (write && editable) fs.writeFileSync(file, fixed, 'utf8');
    const before = original.split(/\r?\n/), after = fixed.split(/\r?\n/);
    console.log(JSON.stringify({ file, utf16, edited: write && editable,
      changes: before.filter((s, i) => s !== after[i]).length,
      samples: before.flatMap((s, i) => s !== after[i] ? [{ line: i + 1, before: s.slice(0, 140), after: after[i].slice(0, 140) }] : []).slice(0, 2) }));
  }
}
walk('Assets');
