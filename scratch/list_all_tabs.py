import urllib.request
import re
import sys
sys.stdout.reconfigure(encoding='utf-8')

url = 'https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/edit?usp=sharing'
req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
with urllib.request.urlopen(req, timeout=15) as resp:
    html = resp.read().decode('utf-8')

tabs = re.findall(r'docs-sheet-tab-caption">([^<]+)</div>', html)
print('ALL TABS IN GOOGLE SHEET:')
for t in tabs:
    print(' -', t)
