import os, sys
sys.stdout.reconfigure(encoding='utf-8')

for root, dirs, files in os.walk('Assets'):
    for f in files:
        if f.endswith('.meta'):
            p = os.path.join(root, f)
            with open(p, 'r', encoding='utf-8', errors='ignore') as mf:
                if 'b07d1587e3525de42b6112a9a8f73ccf' in mf.read():
                    print('Found:', p[:-5])
