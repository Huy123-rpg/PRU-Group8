import urllib.request
import sys
sys.stdout.reconfigure(encoding='utf-8')

url = 'https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/gviz/tq?tqx=out:csv&sheet=Cauhoituluan'

try:
    req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
    with urllib.request.urlopen(req, timeout=15) as resp:
        content = resp.read().decode('utf-8')
        lines = content.splitlines()
        print(f"Cauhoituluan lines: {len(lines)}")
        if lines:
            print("Header:", lines[0])
            print("Line 1:", lines[1] if len(lines) > 1 else "")
        with open('scratch/live_sheet_cauhoituluan.csv', 'w', encoding='utf-8') as f:
            f.write(content)
except Exception as e:
    print("Error fetching Cauhoituluan:", e)
