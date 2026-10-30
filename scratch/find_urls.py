import glob, re, os

urls = set()
for folder in ['Assets', 'Backend', 'scratch']:
    for root, dirs, files in os.walk(folder):
        for file in files:
            if file.endswith(('.cs', '.json', '.gs', '.md', '.txt')):
                p = os.path.join(root, file)
                try:
                    with open(p, 'r', encoding='utf-8', errors='ignore') as s:
                        content = s.read()
                        for m in re.findall(r'https?://[^\s\"\'\<\>\)]+', content):
                            if any(k in m for k in ['script.google.com', 'generativelanguage', 'api.openai', 'googleapis.com']):
                                urls.add((p, m))
                except Exception:
                    pass

for f, u in sorted(urls):
    print(f"{f} -> {u}")
