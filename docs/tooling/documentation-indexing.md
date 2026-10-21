# Maintaining the documentation indexes

The [documentation map](../README.md) identifies current authorities; topic
README files explain current versus proposed behavior. The [complete catalog](../CATALOG.md)
indexes every file under docs, including figures and machine-readable evidence.
The [archive index](../archive/README.md) and [2026-10-21 relocation record](document-relocations-2026-10-21.json)
preserve navigation to superseded work.

When archiving, verify source/destination paths stay within the repository,
retain raw evidence unchanged, update relative Markdown links, and give archived
narratives a status note pointing to the current authority. Keep executable
validator inputs at stable paths unless their consumers are updated as part of
an authorized tooling change. Do not rewrite historical hashes or pending-test
snapshots to pretend the later outcome was already known.

The repository attributes preserve captured gunpowder logs and JSON evidence
byte-for-byte. Whitespace checks are disabled only for those raw console logs,
whose original wrapping/trailing spaces are part of the retained capture;
source code and authored documentation keep their normal checks.

After adding or moving artifacts, regenerate CATALOG.md using this standalone
Python recipe from the repository root (update the displayed review date for
future runs). Save temporary execution files under .tmp, not docs. Then check
all local Markdown links outside fenced code, exact catalog coverage, archive
binary hashes and the implementation-matrix validator. Documentation-only edits
do not require rebuilding or redeploying an unchanged runtime package.

```python
from pathlib import Path
import urllib.parse
R=Path.cwd();D=R/"docs"
def generate_catalog():
 files=sorted([p for p in D.rglob('*') if p.is_file() and p.name!='CATALOG.md'],key=lambda p:p.relative_to(D).as_posix().lower())
 groups={}
 for p in files:
  rel=p.relative_to(D);group='Top-level guides and authorities' if len(rel.parts)==1 else rel.parts[0]
  if 'archive' in rel.parts:group='Archives'
  groups.setdefault(group,[]).append(p)
 parts=['# Complete documentation and artifact catalog','Generated 2026-10-21. This catalog indexes every other file under `docs/`, including machine-readable evidence and images. For interpretation, start with the [documentation map](README.md), [ship-balance hub](ship-balance-research/README.md), or [archive index](archive/README.md). Location/type labels are navigation aids, not assertions that all research is implemented.',f'Indexed files: **{len(files)}**. No archived evidence was deleted during the organization pass.']
 for group,items in groups.items():
  parts+=['\n## '+group+'\n','| File | Type |\n|---|---|']
  for p in items:
   rel=p.relative_to(D).as_posix();label=rel.replace('|','\\|');url=urllib.parse.quote(rel,safe='/')
   parts.append(f'| [{label}]({url}) | {p.suffix.lstrip(".").upper() or "FILE"} |')
 (D/'CATALOG.md').write_text('\n\n'.join(parts[:3])+'\n\n'+'\n'.join(parts[3:])+'\n',encoding='utf-8')
 return files
files=generate_catalog()
print('Catalog files:',len(files))

```
