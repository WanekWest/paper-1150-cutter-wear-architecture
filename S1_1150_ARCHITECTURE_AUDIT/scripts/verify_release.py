"""Verify all payload files against SHA256SUMS.csv. Standard library only."""
from pathlib import Path
import csv,hashlib,sys
root=Path(__file__).resolve().parents[1];bad=[]
with (root/'SHA256SUMS.csv').open(encoding='utf-8',newline='') as f:
    rows=list(csv.DictReader(f))
for r in rows:
    p=(root/r['path']).resolve()
    if not p.is_relative_to(root.resolve()) or not p.is_file():bad.append(r['path']);continue
    if hashlib.sha256(p.read_bytes()).hexdigest()!=r['sha256']:bad.append(r['path'])
print(f'Checked {len(rows)} files; mismatches: {len(bad)}')
if bad:print('\n'.join(bad));sys.exit(1)
