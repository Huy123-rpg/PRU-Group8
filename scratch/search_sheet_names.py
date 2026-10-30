import urllib.request
import re
import sys
sys.stdout.reconfigure(encoding='utf-8')

sheet_id = '1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY'
url = f'https://docs.google.com/spreadsheets/d/{sheet_id}/edit?usp=sharing'

req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
with urllib.request.urlopen(req, timeout=15) as resp:
    html = resp.read().decode('utf-8')

for m in re.finditer(r'NganHangCauHoi', html):
    start = max(0, m.start() - 200)
    end = min(len(html), m.end() + 200)
    print("Found around NganHangCauHoi:")
    print(html[start:end])
    break
